import { EventEmitter } from 'events';
import { Agent, AgentStatus, TSAPIEvent, CallCenterStats } from '../types/agent';
import { logger } from '../utils/logger';
import fetch from 'node-fetch'; // You'll need: npm install node-fetch @types/node-fetch

/**
 * TSAPI Client Wrapper for Node.js
 * 
 * IMPORTANT: This does NOT directly use TSAPI SDK (which is not available as npm package)
 * Instead, it connects to a separate Windows Service or Java application that
 * handles the actual TSAPI integration and exposes REST/WebSocket APIs.
 * 
 * Implementation Options:
 * 1. Windows Service (C#/.NET) with TSAPI SDK -> REST API -> This Node.js client
 * 2. Java Application with JTAPI -> REST API -> This Node.js client  
 * 3. Direct TCP connection to AES (complex, requires CSTA protocol implementation)
 */
export class TSAPIClient extends EventEmitter {
  private isConnected: boolean = false;
  private tsapiServiceUrl: string;
  private monitoredDevices: Set<string> = new Set();
  private reconnectTimer: NodeJS.Timeout | null = null;
  private pollingInterval: NodeJS.Timeout | null = null;
  
  private config = {
    // URL of your Windows Service or Java application that handles TSAPI
    tsapiServiceUrl: process.env.TSAPI_SERVICE_URL || 'http://localhost:8080',
    pollingInterval: parseInt(process.env.POLLING_INTERVAL || '5000'),
    timeout: parseInt(process.env.TSAPI_CLIENT_TIMEOUT || '30000'),
    apiKey: process.env.TSAPI_SERVICE_API_KEY || ''
  };

  constructor() {
    super();
    this.tsapiServiceUrl = this.config.tsapiServiceUrl;
  }

  /**
   * Initialize connection to TSAPI service (Windows Service/Java App)
   */
  async connect(): Promise<boolean> {
    try {
      logger.info('Connecting to TSAPI service...', {
        serviceUrl: this.tsapiServiceUrl
      });

      // Test connection to your TSAPI service
      const response = await fetch(`${this.tsapiServiceUrl}/api/health`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.config.apiKey}`,
          'Content-Type': 'application/json'
        },
        timeout: this.config.timeout
      });

      if (!response.ok) {
        throw new Error(`TSAPI service responded with ${response.status}: ${response.statusText}`);
      }

      const healthData = await response.json();
      logger.info('TSAPI service health check passed', healthData);

      this.isConnected = true;
      
      logger.info('TSAPI service connection established');
      this.emit('connected');
      
      // Start polling for events and data
      this.startPolling();
      
      return true;
    } catch (error) {
      logger.error('Failed to connect to TSAPI service', error);
      this.emit('error', error);
      this.scheduleReconnect();
      return false;
    }
  }

  /**
   * Disconnect from TSAPI service
   */
  async disconnect(): Promise<void> {
    try {
      if (this.reconnectTimer) {
        clearTimeout(this.reconnectTimer);
        this.reconnectTimer = null;
      }

      if (this.pollingInterval) {
        clearInterval(this.pollingInterval);
        this.pollingInterval = null;
      }

      this.isConnected = false;
      this.monitoredDevices.clear();
      
      logger.info('TSAPI service connection closed');
      this.emit('disconnected');
    } catch (error) {
      logger.error('Error disconnecting from TSAPI service', error);
    }
  }

  /**
   * Start polling for events and data from TSAPI service
   */
  private startPolling(): void {
    if (this.pollingInterval) {
      return; // Already polling
    }

    this.pollingInterval = setInterval(async () => {
      try {
        await this.pollForEvents();
        await this.pollForAgentUpdates();
      } catch (error) {
        logger.error('Error during polling', error);
      }
    }, this.config.pollingInterval);

    logger.info(`Started polling TSAPI service every ${this.config.pollingInterval}ms`);
  }

  /**
   * Poll for new events from TSAPI service
   */
  private async pollForEvents(): Promise<void> {
    try {
      const response = await fetch(`${this.tsapiServiceUrl}/api/events/recent`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.config.apiKey}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const events = await response.json();
        events.forEach((event: any) => {
          this.handleTSAPIEvent(event);
        });
      }
    } catch (error) {
      logger.debug('Error polling for events', error);
    }
  }

  /**
   * Poll for agent status updates from TSAPI service
   */
  private async pollForAgentUpdates(): Promise<void> {
    try {
      const response = await fetch(`${this.tsapiServiceUrl}/api/agents/status`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.config.apiKey}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const agentData = await response.json();
        // Process agent status updates
        this.emit('agentStatusUpdate', agentData);
      }
    } catch (error) {
      logger.debug('Error polling for agent updates', error);
    }
  }

  /**
   * Handle TSAPI events from the service
   */
  private handleTSAPIEvent(event: any): void {
    const tsapiEvent: TSAPIEvent = {
      id: event.id || `event-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      timestamp: new Date(event.timestamp),
      type: event.type,
      agentId: event.agentId,
      oldState: event.oldState,
      newState: event.newState,
      details: event.details
    };

    logger.info(`TSAPI Event: ${event.type}`, { agentId: event.agentId });
    this.emit('agentEvent', tsapiEvent);
  }

  /**
   * Schedule reconnection attempt
   */
  private scheduleReconnect(): void {
    if (this.reconnectTimer) {
      return; // Already scheduled
    }

    const reconnectDelay = 5000; // 5 seconds
    
    this.reconnectTimer = setTimeout(async () => {
      this.reconnectTimer = null;
      logger.info('Attempting to reconnect to TSAPI service...');
      await this.connect();
    }, reconnectDelay);
  }

  /**
   * Get current connection status
   */
  public getConnectionStatus(): boolean {
    return this.isConnected;
  }

  /**
   * Get monitored devices
   */
  public getMonitoredDevices(): string[] {
    return Array.from(this.monitoredDevices);
  }

  /**
   * Query agent information
   */
  async queryAgent(agentId: string): Promise<Agent | null> {
    try {
      if (!this.isConnected) {
        throw new Error('TSAPI service not connected');
      }

      const response = await fetch(`${this.tsapiServiceUrl}/api/agents/${agentId}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.config.apiKey}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        return await response.json();
      }
      
      return null;
    } catch (error) {
      logger.error(`Failed to query agent ${agentId}`, error);
      return null;
    }
  }

  /**
   * Get call center statistics
   */
  async getCallCenterStats(): Promise<CallCenterStats | null> {
    try {
      if (!this.isConnected) {
        throw new Error('TSAPI service not connected');
      }

      const response = await fetch(`${this.tsapiServiceUrl}/api/stats`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.config.apiKey}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        return await response.json();
      }
      
      return null;
    } catch (error) {
      logger.error('Failed to get call center statistics', error);
      return null;
    }
  }
}
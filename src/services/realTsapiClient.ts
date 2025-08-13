import { Agent, CallCenterStats, TSAPIEvent } from '../types/agent';

/**
 * Real TSAPI Client for frontend
 * This connects to the backend API server which handles actual TSAPI integration
 */
export class RealTSAPIClient {
  private wsConnection: WebSocket | null = null;
  private eventListeners: ((event: TSAPIEvent) => void)[] = [];
  private statsListeners: ((stats: CallCenterStats) => void)[] = [];
  private connectionListeners: ((connected: boolean) => void)[] = [];
  private reconnectTimer: number | null = null;
  
  private backendUrl: string;
  private wsUrl: string;

  constructor() {
    this.backendUrl = import.meta.env.VITE_BACKEND_URL || 'http://localhost:3001';
    this.wsUrl = import.meta.env.VITE_WS_URL || 'ws://localhost:3001';
    this.connect();
  }

  /**
   * Connect to backend WebSocket
   */
  private connect(): void {
    try {
      this.wsConnection = new WebSocket(this.wsUrl);
      
      this.wsConnection.onopen = () => {
        console.log('Connected to TSAPI backend');
        this.connectionListeners.forEach(listener => listener(true));
        
        // Subscribe to all events
        this.send({ type: 'subscribe_agents' });
        this.send({ type: 'subscribe_events' });
        
        if (this.reconnectTimer) {
          clearTimeout(this.reconnectTimer);
          this.reconnectTimer = null;
        }
      };

      this.wsConnection.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          this.handleMessage(data);
        } catch (error) {
          console.error('Failed to parse WebSocket message:', error);
        }
      };

      this.wsConnection.onclose = () => {
        console.log('Disconnected from TSAPI backend');
        this.connectionListeners.forEach(listener => listener(false));
        this.scheduleReconnect();
      };

      this.wsConnection.onerror = (error) => {
        console.error('WebSocket error:', error);
      };

    } catch (error) {
      console.error('Failed to connect to TSAPI backend:', error);
      this.scheduleReconnect();
    }
  }

  /**
   * Handle incoming WebSocket messages
   */
  private handleMessage(data: any): void {
    switch (data.type) {
      case 'agent_event':
        this.eventListeners.forEach(listener => listener(data.data));
        break;
      
      case 'stats_update':
        this.statsListeners.forEach(listener => listener(data.data));
        break;
      
      case 'tsapi_connected':
        console.log('TSAPI server connected');
        break;
      
      case 'tsapi_disconnected':
        console.log('TSAPI server disconnected');
        break;
      
      case 'tsapi_error':
        console.error('TSAPI error:', data.error);
        break;
      
      default:
        console.log('Unknown message type:', data.type);
    }
  }

  /**
   * Send message to backend
   */
  private send(message: any): void {
    if (this.wsConnection?.readyState === WebSocket.OPEN) {
      this.wsConnection.send(JSON.stringify(message));
    }
  }

  /**
   * Schedule reconnection attempt
   */
  private scheduleReconnect(): void {
    if (this.reconnectTimer) {
      return;
    }

    this.reconnectTimer = window.setTimeout(() => {
      this.reconnectTimer = null;
      console.log('Attempting to reconnect to TSAPI backend...');
      this.connect();
    }, 5000);
  }

  /**
   * Get agents from backend API
   */
  async getAgents(): Promise<Agent[]> {
    try {
      const response = await fetch(`${this.backendUrl}/api/agents`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch agents:', error);
      return [];
    }
  }

  /**
   * Get statistics from backend API
   */
  async getStats(): Promise<CallCenterStats | null> {
    try {
      const response = await fetch(`${this.backendUrl}/api/stats`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch stats:', error);
      return null;
    }
  }

  /**
   * Get events from backend API
   */
  async getEvents(): Promise<TSAPIEvent[]> {
    try {
      const response = await fetch(`${this.backendUrl}/api/events`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch events:', error);
      return [];
    }
  }

  /**
   * Subscribe to TSAPI events
   */
  onEvent(listener: (event: TSAPIEvent) => void): () => void {
    this.eventListeners.push(listener);
    return () => {
      const index = this.eventListeners.indexOf(listener);
      if (index > -1) {
        this.eventListeners.splice(index, 1);
      }
    };
  }

  /**
   * Subscribe to statistics updates
   */
  onStatsUpdate(listener: (stats: CallCenterStats) => void): () => void {
    this.statsListeners.push(listener);
    return () => {
      const index = this.statsListeners.indexOf(listener);
      if (index > -1) {
        this.statsListeners.splice(index, 1);
      }
    };
  }

  /**
   * Subscribe to connection status changes
   */
  onConnectionChange(listener: (connected: boolean) => void): () => void {
    this.connectionListeners.push(listener);
    return () => {
      const index = this.connectionListeners.indexOf(listener);
      if (index > -1) {
        this.connectionListeners.splice(index, 1);
      }
    };
  }

  /**
   * Disconnect and cleanup
   */
  disconnect(): void {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.wsConnection) {
      this.wsConnection.close();
      this.wsConnection = null;
    }

    this.eventListeners = [];
    this.statsListeners = [];
    this.connectionListeners = [];
  }
}

// Export singleton instance
export const realTsapiClient = new RealTSAPIClient();
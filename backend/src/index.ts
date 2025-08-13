import express from 'express';
import cors from 'cors';
import { createServer } from 'http';
import { WebSocketServer } from 'ws';
import dotenv from 'dotenv';
import { TSAPIClient } from './tsapi/client';
import { logger } from './utils/logger';

// Load environment variables
dotenv.config();

const app = express();
const server = createServer(app);
const wss = new WebSocketServer({ server });

// Middleware
app.use(cors({
  origin: process.env.CORS_ORIGIN || 'http://localhost:5173'
}));
app.use(express.json());

// TSAPI Client instance
const tsapiClient = new TSAPIClient();

// WebSocket connections
const wsConnections = new Set<any>();

// TSAPI event handlers
tsapiClient.on('connected', () => {
  logger.info('TSAPI connected - broadcasting to clients');
  broadcast({ type: 'tsapi_connected' });
});

tsapiClient.on('disconnected', () => {
  logger.warn('TSAPI disconnected - broadcasting to clients');
  broadcast({ type: 'tsapi_disconnected' });
});

tsapiClient.on('agentEvent', (event) => {
  logger.debug('Agent event received', event);
  broadcast({ type: 'agent_event', data: event });
});

tsapiClient.on('error', (error) => {
  logger.error('TSAPI error', error);
  broadcast({ type: 'tsapi_error', error: error.message });
});

// WebSocket handling
wss.on('connection', (ws) => {
  wsConnections.add(ws);
  logger.info('WebSocket client connected');

  // Send current connection status
  ws.send(JSON.stringify({
    type: 'connection_status',
    connected: tsapiClient.getConnectionStatus()
  }));

  ws.on('close', () => {
    wsConnections.delete(ws);
    logger.info('WebSocket client disconnected');
  });

  ws.on('message', (message) => {
    try {
      const data = JSON.parse(message.toString());
      handleWebSocketMessage(ws, data);
    } catch (error) {
      logger.error('Invalid WebSocket message', error);
    }
  });
});

// Handle WebSocket messages
function handleWebSocketMessage(ws: any, data: any) {
  switch (data.type) {
    case 'subscribe_agents':
      // Client wants to subscribe to agent updates
      ws.subscriptions = ws.subscriptions || new Set();
      ws.subscriptions.add('agents');
      break;
    
    case 'subscribe_events':
      // Client wants to subscribe to event updates
      ws.subscriptions = ws.subscriptions || new Set();
      ws.subscriptions.add('events');
      break;
    
    default:
      logger.warn('Unknown WebSocket message type', data.type);
  }
}

// Broadcast message to all connected WebSocket clients
function broadcast(message: any) {
  const messageStr = JSON.stringify(message);
  wsConnections.forEach(ws => {
    if (ws.readyState === ws.OPEN) {
      ws.send(messageStr);
    }
  });
}

// REST API Routes
app.get('/api/health', (req, res) => {
  res.json({
    status: 'ok',
    tsapi_connected: tsapiClient.getConnectionStatus(),
    monitored_devices: tsapiClient.getMonitoredDevices().length,
    timestamp: new Date().toISOString()
  });
});

app.get('/api/agents', async (req, res) => {
  try {
    // TODO: Get real agent data from TSAPI
    // For now, return empty array - real implementation would query TSAPI
    res.json([]);
  } catch (error) {
    logger.error('Error fetching agents', error);
    res.status(500).json({ error: 'Failed to fetch agents' });
  }
});

app.get('/api/stats', async (req, res) => {
  try {
    const stats = await tsapiClient.getCallCenterStats();
    res.json(stats || {});
  } catch (error) {
    logger.error('Error fetching stats', error);
    res.status(500).json({ error: 'Failed to fetch statistics' });
  }
});

app.get('/api/events', (req, res) => {
  try {
    // TODO: Return recent events from event store
    res.json([]);
  } catch (error) {
    logger.error('Error fetching events', error);
    res.status(500).json({ error: 'Failed to fetch events' });
  }
});

// Start server
const PORT = process.env.PORT || 3001;

server.listen(PORT, () => {
  logger.info(`Server running on port ${PORT}`);
  
  // Initialize TSAPI connection
  tsapiClient.connect().catch(error => {
    logger.error('Failed to initialize TSAPI connection', error);
  });
});

// Graceful shutdown
process.on('SIGTERM', async () => {
  logger.info('Received SIGTERM, shutting down gracefully');
  await tsapiClient.disconnect();
  server.close(() => {
    logger.info('Server closed');
    process.exit(0);
  });
});

process.on('SIGINT', async () => {
  logger.info('Received SIGINT, shutting down gracefully');
  await tsapiClient.disconnect();
  server.close(() => {
    logger.info('Server closed');
    process.exit(0);
  });
});
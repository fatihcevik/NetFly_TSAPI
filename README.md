# TSAPI Call Center Agent Status Monitoring Application

## 🎯 Overview

This application provides real-time monitoring of call center agent status using Avaya's TSAPI (Telephony Services Application Programming Interface) or JTAPI. It displays live agent states, call statistics, and system events in a professional dashboard interface.

## ⚠️ Important Note About TSAPI Integration

**TSAPI is NOT available as an npm package!** TSAPI is a proprietary Avaya SDK that comes as native Windows/Linux libraries. You have several implementation options:

### Option 1: Windows Service with TSAPI SDK (Recommended)
- Create a separate Windows Service using C#/.NET or C++
- Use Avaya TSAPI SDK (native Windows libraries)
- Expose data via REST API or WebSocket to Node.js backend
- This is the most common production approach

### Option 2: JTAPI Integration (Java-based)
- Use Java with Avaya JTAPI libraries
- Create a Java service that connects to Avaya
- Expose data via REST API to Node.js backend
- More portable than TSAPI but still requires Avaya JTAPI libraries

### Option 3: Direct AES Integration
- Connect directly to Avaya AES server using TCP/IP
- Implement CSTA protocol manually
- Most complex but gives full control

### Option 4: Third-party Middleware
- Use existing middleware solutions like Avaya Breeze
- Connect through their APIs
- Requires additional licensing

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Web     │    │   Node.js API    │    │  Windows Service │    │   Avaya AES     │
│   Dashboard     │◄──►│   Server         │◄──►│  (C#/Java)      │◄──►│   TSAPI Server  │
└─────────────────┘    └──────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🔧 Prerequisites

### Avaya Infrastructure
- **Avaya Aura Application Enablement Services (AES)** - TSAPI service host
- **TSAPI Client** - Must be installed on Windows server
- **TSAPI SDK** - Native Windows/Linux libraries (not npm package!)
- **Valid TSAPI License** - From Avaya or authorized partner

### Development Environment
- Node.js 18+ 
- npm or yarn
- TypeScript support
- **Windows Server** (for TSAPI SDK) or Java environment (for JTAPI)
- Access to Avaya AES server

## 🚨 TSAPI SDK Integration Reality Check

The TSAPI SDK is **NOT** a Node.js package. It's a native Windows/Linux library that requires:

1. **Windows/Linux Installation**: Physical installation on the server
2. **Native Code**: C/C++, C#, or Java integration
3. **Licensing**: Proper Avaya licensing and permissions
4. **Network Access**: Direct connection to Avaya AES server

### Current Implementation Status
- ✅ **Frontend Dashboard**: Complete and functional
- ✅ **Node.js API Structure**: Ready for integration
- ❌ **TSAPI SDK Integration**: Requires native implementation
- ✅ **Simulator**: Working for development/demo

## 📦 Installation

### 1. Clone and Setup Web Application
```bash
git clone <repository-url>
cd tsapi-call-center-monitoring
npm install
```

### 2. Choose Your Integration Approach

#### Option A: Windows Service Approach (Recommended)
```bash
# 1. Create a separate Windows Service project
# 2. Install Avaya TSAPI SDK on Windows server
# 3. Implement TSAPI integration in C#/.NET
# 4. Expose REST API endpoints
# 5. Configure Node.js backend to consume the API
```

#### Option B: Java JTAPI Approach
```bash
# 1. Create Java application with JTAPI libraries
# 2. Install Avaya JTAPI on server
# 3. Implement JTAPI integration
# 4. Expose REST API endpoints
# 5. Configure Node.js backend to consume the API
```

### 3. Setup Backend API Server
```bash
cd backend
npm install
cp .env.example .env
# Configure connection to your TSAPI service (not direct TSAPI)
```
## ⚙️ Configuration

### Environment Variables (.env)
```env
# TSAPI Connection
TSAPI_SERVER_HOST=your-aes-server.company.com
TSAPI_SERVER_PORT=450
TSAPI_APPLICATION_NAME=CallCenterMonitor
TSAPI_USERNAME=tsapi_user
TSAPI_PASSWORD=tsapi_password
TSAPI_CLIENT_TIMEOUT=30000

# Database (Optional - for logging)
DATABASE_URL=postgresql://user:pass@localhost:5432/callcenter

# Web Server
PORT=3001
CORS_ORIGIN=http://localhost:5173
```

### TSAPI Security Database Configuration
```sql
-- Grant device access permissions
INSERT INTO TSAPI_SECURITY (username, device_id, permissions)
VALUES ('tsapi_user', 'AGENT_DEVICE_*', 'MONITOR,QUERY');
```

## 🚀 Running the Application

### Development Mode
```bash
# Start backend API server
cd backend
npm run dev

# Start frontend (in another terminal)
npm run dev
```

### Production Mode
```bash
# Build frontend
npm run build

# Start production server
cd backend
npm start
```

## 📊 Features

### Real-Time Agent Monitoring
- **Agent Status Tracking**: Available, Busy, ACW, Not Ready, On Call
- **Live Status Updates**: Real-time state changes via TSAPI events
- **Agent Details**: Extension, skill groups, call statistics
- **Status Duration**: Time in current state

### Call Center Statistics
- **Queue Metrics**: Calls waiting, average wait time
- **Service Level**: Real-time SLA performance
- **Agent Utilization**: Available vs busy ratios
- **Historical Data**: Trend analysis and reporting

### Event Logging
- **TSAPI Events**: AgentLoggedOn, AgentLoggedOff, AgentStateChanged
- **Real-time Feed**: Live event stream with timestamps
- **Event History**: Searchable event log
- **Alert System**: Configurable notifications

## 🔌 TSAPI Integration Details

### Supported TSAPI Events
```typescript
// Agent Events
- cstaAgentLoggedOn
- cstaAgentLoggedOff  
- cstaAgentWorkMode
- cstaAgentStateChanged

// Call Events
- cstaServiceInitiated
- cstaDelivered
- cstaEstablished
- cstaConnectionCleared

// System Events
- cstaMonitorStarted
- cstaMonitorStopped
```

### Device Monitoring
```typescript
// Monitor agent devices
const devices = [
  'AGENT_001', 'AGENT_002', 'AGENT_003'
  // ... all agent extensions
];

devices.forEach(device => {
  tsapi.monitorDevice(device, {
    events: ['all'],
    filter: 'agent_events'
  });
});
```

## 🛠️ Development

### Project Structure
```
src/
├── components/          # React UI components
│   ├── AgentTable.tsx
│   ├── StatsOverview.tsx
│   ├── EventLog.tsx
│   └── RealTimeChart.tsx
├── services/           # API and TSAPI services
│   ├── tsapiClient.ts  # Real TSAPI integration
│   ├── apiClient.ts    # Backend API client
│   └── simulator.ts    # Development simulator
├── types/              # TypeScript definitions
│   └── agent.ts
└── hooks/              # React hooks
    └── useTSAPI.ts

backend/
├── src/
│   ├── tsapi/          # TSAPI integration
│   │   ├── client.ts   # TSAPI client wrapper
│   │   ├── events.ts   # Event handlers
│   │   └── monitor.ts  # Device monitoring
│   ├── api/            # REST API endpoints
│   └── websocket/      # Real-time updates
└── package.json
```

### Adding New Agent States
```typescript
// 1. Update type definition
export type AgentStatus = 
  | 'available'
  | 'busy' 
  | 'acw'
  | 'not-ready'
  | 'on-call'
  | 'your-new-state';  // Add here

// 2. Update status configuration
const statusConfig = {
  'your-new-state': { 
    label: 'Your Label', 
    color: 'bg-purple-500 text-white' 
  }
};

// 3. Handle in TSAPI event processor
```

## 🔒 Security Considerations

### TSAPI Security
- Use dedicated TSAPI service account
- Limit device access permissions
- Enable TSAPI encryption (TLS)
- Regular password rotation

### Application Security
- Environment variable protection
- API authentication/authorization
- CORS configuration
- Input validation and sanitization

## 📈 Performance Optimization

### TSAPI Connection Management
```typescript
// Connection pooling
const tsapiPool = new TSAPIConnectionPool({
  maxConnections: 5,
  reconnectInterval: 5000,
  heartbeatInterval: 30000
});

// Event batching
const eventBatcher = new EventBatcher({
  batchSize: 100,
  flushInterval: 1000
});
```

### Frontend Optimization
- Virtual scrolling for large agent lists
- Debounced updates for high-frequency events
- Memoized components for performance
- WebSocket connection management

## 🐛 Troubleshooting

### Common TSAPI Issues

#### Connection Failed
```bash
Error: TSERVER_DEVICE_NOT_SUPPORTED
Solution: Check TSAPI Security Database permissions
```

#### Authentication Error
```bash
Error: TSERVER_INVALID_LOGIN
Solution: Verify username/password and AES configuration
```

#### Device Monitoring Failed
```bash
Error: TSERVER_MONITOR_FAILED
Solution: Ensure device exists and has proper permissions
```

### Debug Mode
```bash
# Enable TSAPI debug logging
export TSAPI_DEBUG=1
npm run dev
```

## 📚 API Documentation

### REST Endpoints
```
GET  /api/agents          # Get all agents
GET  /api/agents/:id      # Get specific agent
GET  /api/stats           # Get call center statistics
GET  /api/events          # Get recent events
POST /api/agents/:id/state # Change agent state (if supported)
```

### WebSocket Events
```typescript
// Client -> Server
'subscribe_agents'        # Subscribe to agent updates
'subscribe_stats'         # Subscribe to statistics
'subscribe_events'        # Subscribe to event feed

// Server -> Client  
'agent_update'           # Agent status changed
'stats_update'           # Statistics updated
'new_event'              # New TSAPI event
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

### Avaya Resources
- [Avaya DevConnect](https://developers.avaya.com/)
- [TSAPI SDK Documentation](https://support.avaya.com/)
- [AES Administration Guide](https://documentation.avaya.com/)

### Community
- GitHub Issues for bug reports
- Stack Overflow for development questions
- Avaya Community Forums

## 🔄 Changelog

### v1.0.0 (Current)
- Initial release with basic agent monitoring
- Real-time dashboard with statistics
- TSAPI event logging
- WebSocket real-time updates

### Planned Features
- Historical reporting and analytics
- Advanced alerting and notifications
- Multi-site support
- Mobile responsive improvements
- Integration with other Avaya applications

---

**Note**: This application requires proper Avaya licensing and infrastructure. Contact your Avaya representative for TSAPI SDK access and licensing information.
# TSAPI Windows Service Implementation Example

## Overview
Since TSAPI SDK is not available as an npm package, you need to create a separate Windows Service that handles the actual TSAPI integration and exposes REST APIs for the Node.js backend to consume.

## Option 1: C# .NET Windows Service

### 1. Create Windows Service Project
```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TSAPIService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<TSAPIWorkerService>();
                    services.AddSingleton<ITSAPIClient, TSAPIClient>();
                    services.AddControllers();
                });
    }
}
```

### 2. TSAPI Client Implementation
```csharp
// TSAPIClient.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avaya.TSAPI; // This comes from Avaya TSAPI SDK

public interface ITSAPIClient
{
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    event EventHandler<AgentEventArgs> AgentStateChanged;
    Task<List<Agent>> GetAgentsAsync();
    Task<CallCenterStats> GetStatsAsync();
}

public class TSAPIClient : ITSAPIClient
{
    private CSTA csta;
    private bool isConnected = false;
    
    public event EventHandler<AgentEventArgs> AgentStateChanged;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            // Initialize TSAPI connection
            csta = new CSTA();
            
            // Configure connection parameters
            var connectionParams = new ConnectionParams
            {
                ServerName = "your-aes-server",
                ApplicationName = "CallCenterMonitor",
                Username = "tsapi_user",
                Password = "tsapi_password"
            };

            // Open TSAPI stream
            await csta.acsOpenStreamAsync(connectionParams);
            
            // Setup event handlers
            csta.AgentLoggedOn += OnAgentLoggedOn;
            csta.AgentLoggedOff += OnAgentLoggedOff;
            csta.AgentStateChanged += OnAgentStateChanged;
            
            // Start monitoring devices
            await StartDeviceMonitoringAsync();
            
            isConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TSAPI Connection failed: {ex.Message}");
            return false;
        }
    }

    private async Task StartDeviceMonitoringAsync()
    {
        var devices = new[] { "AGENT_001", "AGENT_002", "AGENT_003" }; // Your agent devices
        
        foreach (var device in devices)
        {
            try
            {
                await csta.cstaMonitorDeviceAsync(new MonitorDeviceRequest
                {
                    DeviceObject = new DeviceID { DeviceIdentifier = device },
                    MonitorFilter = new CSTAMonitorFilter
                    {
                        Call = true,
                        Agent = true,
                        Feature = true
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to monitor device {device}: {ex.Message}");
            }
        }
    }

    private void OnAgentStateChanged(object sender, CSTAAgentStateChangedEvent e)
    {
        AgentStateChanged?.Invoke(this, new AgentEventArgs
        {
            AgentId = e.AgentID,
            OldState = MapTSAPIState(e.OldAgentState),
            NewState = MapTSAPIState(e.NewAgentState),
            Timestamp = DateTime.Now
        });
    }

    private void OnAgentLoggedOn(object sender, CSTAAgentLoggedOnEvent e)
    {
        AgentStateChanged?.Invoke(this, new AgentEventArgs
        {
            AgentId = e.AgentID,
            NewState = "logged-on",
            Timestamp = DateTime.Now
        });
    }

    private void OnAgentLoggedOff(object sender, CSTAAgentLoggedOffEvent e)
    {
        AgentStateChanged?.Invoke(this, new AgentEventArgs
        {
            AgentId = e.AgentID,
            NewState = "logged-off",
            Timestamp = DateTime.Now
        });
    }

    private string MapTSAPIState(AgentState tsapiState)
    {
        return tsapiState switch
        {
            AgentState.AS_LOG_IN => "logged-on",
            AgentState.AS_LOG_OUT => "logged-off",
            AgentState.AS_NOT_READY => "not-ready",
            AgentState.AS_READY => "available",
            AgentState.AS_WORK_NOT_READY => "busy",
            AgentState.AS_WORK_READY => "acw",
            _ => "unknown"
        };
    }

    public async Task<List<Agent>> GetAgentsAsync()
    {
        // Query agent information from TSAPI
        var agents = new List<Agent>();
        
        // Implementation depends on your specific TSAPI SDK version
        // This is a simplified example
        
        return agents;
    }

    public async Task<CallCenterStats> GetStatsAsync()
    {
        // Query call center statistics from TSAPI
        return new CallCenterStats
        {
            // Populate with real data from TSAPI queries
        };
    }

    public async Task DisconnectAsync()
    {
        if (csta != null && isConnected)
        {
            await csta.acsCloseStreamAsync();
            isConnected = false;
        }
    }
}
```

### 3. REST API Controller
```csharp
// Controllers/TSAPIController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api")]
public class TSAPIController : ControllerBase
{
    private readonly ITSAPIClient _tsapiClient;
    private static readonly List<AgentEventArgs> _recentEvents = new();

    public TSAPIController(ITSAPIClient tsapiClient)
    {
        _tsapiClient = tsapiClient;
        _tsapiClient.AgentStateChanged += OnAgentStateChanged;
    }

    private void OnAgentStateChanged(object sender, AgentEventArgs e)
    {
        _recentEvents.Insert(0, e);
        if (_recentEvents.Count > 100)
        {
            _recentEvents.RemoveAt(_recentEvents.Count - 1);
        }
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "ok", timestamp = DateTime.Now });
    }

    [HttpGet("agents")]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _tsapiClient.GetAgentsAsync();
        return Ok(agents);
    }

    [HttpGet("agents/{id}")]
    public async Task<IActionResult> GetAgent(string id)
    {
        var agents = await _tsapiClient.GetAgentsAsync();
        var agent = agents.FirstOrDefault(a => a.Id == id);
        
        if (agent == null)
            return NotFound();
            
        return Ok(agent);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _tsapiClient.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("events/recent")]
    public IActionResult GetRecentEvents()
    {
        return Ok(_recentEvents.Take(50));
    }
}
```

### 4. Data Models
```csharp
// Models/Agent.cs
public class Agent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public List<string> SkillGroups { get; set; }
    public string Status { get; set; }
    public DateTime LastStatusChange { get; set; }
    public int TotalCallsToday { get; set; }
    public int TotalTalkTime { get; set; }
    public int TotalIdleTime { get; set; }
    public int? CurrentCallDuration { get; set; }
}

public class CallCenterStats
{
    public int TotalAgents { get; set; }
    public int AgentsLoggedOn { get; set; }
    public int AgentsAvailable { get; set; }
    public int AgentsBusy { get; set; }
    public int AgentsInACW { get; set; }
    public int AgentsNotReady { get; set; }
    public int CallsInQueue { get; set; }
    public int AverageWaitTime { get; set; }
    public int LongestWaitTime { get; set; }
    public int CallsAnswered { get; set; }
    public int CallsAbandoned { get; set; }
    public int ServiceLevel { get; set; }
}

public class AgentEventArgs : EventArgs
{
    public string AgentId { get; set; }
    public string OldState { get; set; }
    public string NewState { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Option 2: Java JTAPI Implementation

### 1. Java JTAPI Service
```java
// TSAPIService.java
import javax.telephony.*;
import javax.telephony.callcenter.*;
import com.avaya.jtapi.tsapi.*;

public class TSAPIService {
    private Provider provider;
    private boolean isConnected = false;
    
    public boolean connect() {
        try {
            // Get JTAPI provider
            provider = JtapiPeerFactory.getJtapiPeer().getProvider("your-aes-server");
            
            // Add observer for agent events
            provider.addObserver(new AgentObserver());
            
            isConnected = true;
            return true;
        } catch (Exception e) {
            System.err.println("JTAPI Connection failed: " + e.getMessage());
            return false;
        }
    }
    
    private class AgentObserver implements CallCenterProviderObserver {
        @Override
        public void providerEventTransmissionEnded(ProviderEvent[] events) {
            for (ProviderEvent event : events) {
                if (event instanceof AgentTerminalEvent) {
                    handleAgentEvent((AgentTerminalEvent) event);
                }
            }
        }
        
        private void handleAgentEvent(AgentTerminalEvent event) {
            // Process agent state changes
            // Send to REST API or message queue
        }
    }
}
```

## Installation and Deployment

### 1. Install as Windows Service
```bash
# Using sc command
sc create "TSAPI Service" binPath="C:\path\to\your\service.exe"
sc start "TSAPI Service"
```

### 2. Configure Node.js Backend
```bash
# Update .env file
TSAPI_SERVICE_URL=http://localhost:8080
TSAPI_SERVICE_API_KEY=your-secure-api-key

# Install dependencies
npm install node-fetch @types/node-fetch
```

### 3. Test Integration
```bash
# Test Windows Service API
curl http://localhost:8080/api/health

# Test Node.js backend
curl http://localhost:3001/api/agents
```

## Security Considerations

1. **API Authentication**: Use API keys or JWT tokens
2. **Network Security**: Use HTTPS for production
3. **TSAPI Permissions**: Configure proper device access in TSAPI Security Database
4. **Service Account**: Run Windows Service with dedicated service account
5. **Firewall Rules**: Configure appropriate network access

## Troubleshooting

### Common Issues
1. **TSERVER_DEVICE_NOT_SUPPORTED**: Check TSAPI Security Database permissions
2. **Connection Timeout**: Verify AES server connectivity
3. **Authentication Failed**: Check TSAPI username/password
4. **Service Won't Start**: Check Windows Event Log for details

This approach separates the TSAPI integration concerns from the Node.js web application, providing a clean architecture that's easier to maintain and deploy.
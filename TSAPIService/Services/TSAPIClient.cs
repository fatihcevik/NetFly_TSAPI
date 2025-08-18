using System.Runtime.InteropServices;
using TSAPIService.Models;
using System.Collections.Concurrent;

namespace TSAPIService.Services;

public class TSAPIClient : ITSAPIClient, IDisposable
{
    private readonly ILogger<TSAPIClient> _logger;
    private readonly IConfiguration _configuration;
    
    private bool _isConnected = false;
    private int _acsHandle = 0;
    private readonly ConcurrentDictionary<string, Agent> _agents = new();
    private readonly ConcurrentDictionary<string, Call> _calls = new();
    private readonly List<string> _monitoredDevices = new();
    private readonly object _lockObject = new();
    
    // TSAPI DLL Import'ları
    [DllImport("csta32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int acsOpenStream(ref int acsHandle, int invokeId, string serverName, 
        string loginId, string passwd, string applicationName, int acsLevelReq, 
        int apiVer, int sendQSize, int sendExtraBufs, int recvQSize, int recvExtraBufs);

    [DllImport("csta32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int acsCloseStream(int acsHandle, int invokeId);

    [DllImport("csta32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int acsGetEventPoll(int acsHandle, ref IntPtr eventBuf, ref int numEvents);

    [DllImport("csta32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int cstaMonitorDevice(int acsHandle, int invokeId, string deviceId, 
        ref int monitorCrossRefId);

    [DllImport("csta32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int cstaMonitorStop(int acsHandle, int invokeId, int monitorCrossRefId);

    [DllImport("attprv32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int attQueryAgentState(int acsHandle, int invokeId, string device);

    [DllImport("attprv32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int attSetAgentState(int acsHandle, int invokeId, string device, 
        int agentState, int agentMode, string agentId, string agentGroup, string agentPassword);

    public bool IsConnected => _isConnected;

    public event EventHandler<TSAPIEvent>? AgentEvent;
    public event EventHandler<TSAPIEvent>? CallEvent;
    public event EventHandler<TSAPIEvent>? SystemEvent;

    public TSAPIClient(ILogger<TSAPIClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            var serverName = _configuration["TSAPI:ServerName"] ?? "localhost";
            var loginId = _configuration["TSAPI:LoginId"] ?? "tsapi_user";
            var password = _configuration["TSAPI:Password"] ?? "password";
            var applicationName = _configuration["TSAPI:ApplicationName"] ?? "TSAPIService";

            _logger.LogInformation("TSAPI bağlantısı kuruluyor: {ServerName}", serverName);

            var result = acsOpenStream(ref _acsHandle, 0, serverName, loginId, password, 
                applicationName, 1, 1, 100, 10, 100, 10);

            if (result == 0) // ACSPOSITIVE_ACK
            {
                _isConnected = true;
                _logger.LogInformation("TSAPI bağlantısı başarılı. Handle: {Handle}", _acsHandle);
                
                // Event polling'i başlat
                _ = Task.Run(EventPollingLoop);
                
                // Varsayılan cihazları izlemeye başla
                await StartDefaultMonitoringAsync();
                
                return true;
            }
            else
            {
                _logger.LogError("TSAPI bağlantısı başarısız. Hata kodu: {ErrorCode}", result);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TSAPI bağlantısı sırasında hata");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_isConnected && _acsHandle != 0)
            {
                var result = acsCloseStream(_acsHandle, 0);
                _isConnected = false;
                _acsHandle = 0;
                
                _logger.LogInformation("TSAPI bağlantısı kapatıldı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TSAPI bağlantısı kapatılırken hata");
        }
    }

    private async Task StartDefaultMonitoringAsync()
    {
        var devices = _configuration.GetSection("TSAPI:MonitorDevices").Get<string[]>() ?? Array.Empty<string>();
        
        foreach (var device in devices)
        {
            await StartMonitoringAsync(device);
        }
    }

    private async Task EventPollingLoop()
    {
        while (_isConnected)
        {
            try
            {
                IntPtr eventBuf = IntPtr.Zero;
                int numEvents = 0;
                
                var result = acsGetEventPoll(_acsHandle, ref eventBuf, ref numEvents);
                
                if (result == 0 && numEvents > 0)
                {
                    ProcessEvents(eventBuf, numEvents);
                }
                
                await Task.Delay(100); // 100ms polling interval
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event polling sırasında hata");
                await Task.Delay(1000);
            }
        }
    }

    private void ProcessEvents(IntPtr eventBuf, int numEvents)
    {
        // Bu kısım gerçek TSAPI event yapısına göre implement edilmeli
        // Şimdilik örnek implementation
        
        for (int i = 0; i < numEvents; i++)
        {
            try
            {
                // Event parsing logic burada olacak
                // Her event türü için ayrı handler
                
                var tsapiEvent = new TSAPIEvent
                {
                    Type = TSAPIEventType.SystemEvent,
                    Details = "Event processed",
                    Timestamp = DateTime.UtcNow
                };
                
                SystemEvent?.Invoke(this, tsapiEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event işlenirken hata");
            }
        }
    }

    public async Task<bool> LoginAgentAsync(string agentId, string password)
    {
        try
        {
            if (!_isConnected) return false;

            // ATT private data kullanarak agent login
            var result = attSetAgentState(_acsHandle, 0, agentId, 1, 1, agentId, "", password);
            
            if (result == 0)
            {
                var agent = new Agent
                {
                    Id = agentId,
                    Extension = agentId,
                    Name = $"Agent {agentId}",
                    Status = AgentStatus.LoggedOn,
                    LastStatusChange = DateTime.UtcNow,
                    IsLoggedIn = true
                };
                
                _agents.AddOrUpdate(agentId, agent, (key, oldValue) => agent);
                
                var agentEvent = new TSAPIEvent
                {
                    Type = TSAPIEventType.AgentLoggedOn,
                    AgentId = agentId,
                    NewState = AgentStatus.LoggedOn.ToString(),
                    Details = $"Agent {agentId} logged in"
                };
                
                AgentEvent?.Invoke(this, agentEvent);
                
                _logger.LogInformation("Agent {AgentId} giriş yaptı", agentId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} giriş yaparken hata", agentId);
            return false;
        }
    }

    public async Task<bool> LogoutAgentAsync(string agentId)
    {
        try
        {
            if (!_isConnected) return false;

            var result = attSetAgentState(_acsHandle, 0, agentId, 0, 0, agentId, "", "");
            
            if (result == 0)
            {
                if (_agents.TryGetValue(agentId, out var agent))
                {
                    agent.Status = AgentStatus.LoggedOff;
                    agent.IsLoggedIn = false;
                    agent.LastStatusChange = DateTime.UtcNow;
                }
                
                var agentEvent = new TSAPIEvent
                {
                    Type = TSAPIEventType.AgentLoggedOff,
                    AgentId = agentId,
                    NewState = AgentStatus.LoggedOff.ToString(),
                    Details = $"Agent {agentId} logged out"
                };
                
                AgentEvent?.Invoke(this, agentEvent);
                
                _logger.LogInformation("Agent {AgentId} çıkış yaptı", agentId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} çıkış yaparken hata", agentId);
            return false;
        }
    }

    public async Task<bool> SetAgentStateAsync(string agentId, AgentStatus state)
    {
        try
        {
            if (!_isConnected) return false;

            int tsapiState = state switch
            {
                AgentStatus.Available => 2,
                AgentStatus.NotReady => 3,
                AgentStatus.ACW => 4,
                _ => 2
            };

            var result = attSetAgentState(_acsHandle, 0, agentId, tsapiState, 1, agentId, "", "");
            
            if (result == 0)
            {
                if (_agents.TryGetValue(agentId, out var agent))
                {
                    var oldState = agent.Status;
                    agent.Status = state;
                    agent.LastStatusChange = DateTime.UtcNow;
                    
                    var agentEvent = new TSAPIEvent
                    {
                        Type = TSAPIEventType.AgentStateChanged,
                        AgentId = agentId,
                        OldState = oldState.ToString(),
                        NewState = state.ToString(),
                        Details = $"Agent {agentId} state changed from {oldState} to {state}"
                    };
                    
                    AgentEvent?.Invoke(this, agentEvent);
                }
                
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} durum değiştirirken hata", agentId);
            return false;
        }
    }

    public async Task<Agent?> GetAgentAsync(string agentId)
    {
        _agents.TryGetValue(agentId, out var agent);
        return agent;
    }

    public async Task<List<Agent>> GetAllAgentsAsync()
    {
        return _agents.Values.ToList();
    }

    public async Task<bool> StartMonitoringAsync(string deviceId)
    {
        try
        {
            if (!_isConnected) return false;

            int monitorId = 0;
            var result = cstaMonitorDevice(_acsHandle, 0, deviceId, ref monitorId);
            
            if (result == 0)
            {
                lock (_lockObject)
                {
                    if (!_monitoredDevices.Contains(deviceId))
                    {
                        _monitoredDevices.Add(deviceId);
                    }
                }
                
                _logger.LogInformation("Device {DeviceId} izlemeye alındı", deviceId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Device {DeviceId} izlemeye alınırken hata", deviceId);
            return false;
        }
    }

    public async Task<bool> StopMonitoringAsync(string deviceId)
    {
        try
        {
            lock (_lockObject)
            {
                _monitoredDevices.Remove(deviceId);
            }
            
            _logger.LogInformation("Device {DeviceId} izlemeden çıkarıldı", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Device {DeviceId} izlemeden çıkarılırken hata", deviceId);
            return false;
        }
    }

    public async Task<List<string>> GetMonitoredDevicesAsync()
    {
        lock (_lockObject)
        {
            return new List<string>(_monitoredDevices);
        }
    }

    // Call operations - Basit implementasyonlar
    public async Task<bool> MakeCallAsync(string agentId, string destination) => false;
    public async Task<bool> AnswerCallAsync(string callId) => false;
    public async Task<bool> HangupCallAsync(string callId) => false;
    public async Task<bool> HoldCallAsync(string callId) => false;
    public async Task<bool> RetrieveCallAsync(string callId) => false;
    public async Task<bool> TransferCallAsync(string callId, string destination) => false;

    public async Task<CallCenterStats> GetStatsAsync()
    {
        var agents = _agents.Values.ToList();
        
        return new CallCenterStats
        {
            TotalAgents = agents.Count,
            AgentsLoggedOn = agents.Count(a => a.IsLoggedIn),
            AgentsAvailable = agents.Count(a => a.Status == AgentStatus.Available),
            AgentsBusy = agents.Count(a => a.Status == AgentStatus.Busy || a.Status == AgentStatus.OnCall),
            AgentsInACW = agents.Count(a => a.Status == AgentStatus.ACW),
            AgentsNotReady = agents.Count(a => a.Status == AgentStatus.NotReady),
            ActiveCalls = _calls.Count,
            ServiceLevel = 95.0,
            LastUpdated = DateTime.UtcNow
        };
    }

    public TSAPIConnectionInfo GetConnectionInfo()
    {
        return new TSAPIConnectionInfo
        {
            IsConnected = _isConnected,
            ServerName = _configuration["TSAPI:ServerName"] ?? "",
            ApplicationName = _configuration["TSAPI:ApplicationName"] ?? "",
            ConnectedAt = _isConnected ? DateTime.UtcNow : null,
            MonitoredDevices = _monitoredDevices.Count
        };
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
}
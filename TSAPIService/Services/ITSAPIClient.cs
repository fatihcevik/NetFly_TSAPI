using TSAPIService.Models;

namespace TSAPIService.Services;

public interface ITSAPIClient
{
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected { get; }
    
    // Event handlers
    event EventHandler<TSAPIEvent>? AgentEvent;
    event EventHandler<TSAPIEvent>? CallEvent;
    event EventHandler<TSAPIEvent>? SystemEvent;
    
    // Agent operations
    Task<bool> LoginAgentAsync(string agentId, string password);
    Task<bool> LogoutAgentAsync(string agentId);
    Task<bool> SetAgentStateAsync(string agentId, AgentStatus state);
    Task<Agent?> GetAgentAsync(string agentId);
    Task<List<Agent>> GetAllAgentsAsync();
    
    // Call operations
    Task<bool> MakeCallAsync(string agentId, string destination);
    Task<bool> AnswerCallAsync(string callId);
    Task<bool> HangupCallAsync(string callId);
    Task<bool> HoldCallAsync(string callId);
    Task<bool> RetrieveCallAsync(string callId);
    Task<bool> TransferCallAsync(string callId, string destination);
    
    // Monitoring
    Task<bool> StartMonitoringAsync(string deviceId);
    Task<bool> StopMonitoringAsync(string deviceId);
    Task<List<string>> GetMonitoredDevicesAsync();
    
    // Statistics
    Task<CallCenterStats> GetStatsAsync();
    TSAPIConnectionInfo GetConnectionInfo();
}
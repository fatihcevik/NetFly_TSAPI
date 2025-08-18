using System.Text.Json.Serialization;

namespace TSAPIService.Models;

public class Agent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public List<string> SkillGroups { get; set; } = new();
    public AgentStatus Status { get; set; }
    public DateTime LastStatusChange { get; set; }
    public int TotalCallsToday { get; set; }
    public int TotalTalkTime { get; set; }
    public int TotalIdleTime { get; set; }
    public int? CurrentCallDuration { get; set; }
    public bool IsLoggedIn { get; set; }
    public string? CurrentCallId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentStatus
{
    LoggedOff,
    LoggedOn,
    Available,
    Busy,
    ACW,
    NotReady,
    OnCall
}

public class Call
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string CallerNumber { get; set; } = string.Empty;
    public string CalledNumber { get; set; } = string.Empty;
    public CallState State { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? AnswerTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public CallDirection Direction { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CallState
{
    Initiated,
    Delivered,
    Established,
    Held,
    Transferred,
    Conferenced,
    Cleared
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CallDirection
{
    Inbound,
    Outbound,
    Internal
}

public class TSAPIEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TSAPIEventType Type { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string? CallId { get; set; }
    public string? OldState { get; set; }
    public string? NewState { get; set; }
    public string Details { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TSAPIEventType
{
    AgentLoggedOn,
    AgentLoggedOff,
    AgentStateChanged,
    AgentWorkMode,
    CallDelivered,
    CallEstablished,
    CallCleared,
    CallHeld,
    CallRetrieved,
    CallTransferred,
    CallConferenced,
    SystemEvent
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
    public double ServiceLevel { get; set; }
    public int ActiveCalls { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class TSAPIConnectionInfo
{
    public bool IsConnected { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public DateTime? ConnectedAt { get; set; }
    public int MonitoredDevices { get; set; }
    public string LastError { get; set; } = string.Empty;
}
using Microsoft.AspNetCore.SignalR;
using TSAPIService.Models;
using TSAPIService.Services;

namespace TSAPIService.Hubs;

public class TSAPIHub : Hub
{
    private readonly IAgentService _agentService;
    private readonly ICallService _callService;
    private readonly IEventService _eventService;
    private readonly ILogger<TSAPIHub> _logger;

    public TSAPIHub(
        IAgentService agentService,
        ICallService callService,
        IEventService eventService,
        ILogger<TSAPIHub> logger)
    {
        _agentService = agentService;
        _callService = callService;
        _eventService = eventService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client bağlandı: {ConnectionId}", Context.ConnectionId);
        
        // Client'a mevcut durumu gönder
        var agents = await _agentService.GetAllAgentsAsync();
        var stats = await _agentService.GetStatsAsync();
        var recentEvents = await _eventService.GetRecentEventsAsync(10);

        await Clients.Caller.SendAsync("AgentsUpdate", agents);
        await Clients.Caller.SendAsync("StatsUpdate", stats);
        await Clients.Caller.SendAsync("RecentEvents", recentEvents);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client bağlantısı kesildi: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // Client'tan gelen komutlar
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} {GroupName} grubuna katıldı", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} {GroupName} grubundan ayrıldı", Context.ConnectionId, groupName);
    }

    public async Task SubscribeToAgent(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        _logger.LogInformation("Client {ConnectionId} agent {AgentId} eventlerini dinliyor", Context.ConnectionId, agentId);
    }

    public async Task UnsubscribeFromAgent(string agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        _logger.LogInformation("Client {ConnectionId} agent {AgentId} eventlerini dinlemeyi bıraktı", Context.ConnectionId, agentId);
    }

    // Agent işlemleri
    public async Task<bool> LoginAgent(string agentId, string password)
    {
        try
        {
            var result = await _agentService.LoginAgentAsync(agentId, password);
            if (result)
            {
                await Clients.All.SendAsync("AgentLoggedIn", agentId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent login hatası");
            return false;
        }
    }

    public async Task<bool> LogoutAgent(string agentId)
    {
        try
        {
            var result = await _agentService.LogoutAgentAsync(agentId);
            if (result)
            {
                await Clients.All.SendAsync("AgentLoggedOut", agentId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent logout hatası");
            return false;
        }
    }

    public async Task<bool> SetAgentState(string agentId, AgentStatus state)
    {
        try
        {
            var result = await _agentService.SetAgentStateAsync(agentId, state);
            if (result)
            {
                await Clients.All.SendAsync("AgentStateChanged", agentId, state);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent state değiştirme hatası");
            return false;
        }
    }

    // Call işlemleri
    public async Task<bool> MakeCall(string agentId, string destination)
    {
        try
        {
            return await _callService.MakeCallAsync(agentId, destination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arama yapma hatası");
            return false;
        }
    }

    public async Task<bool> AnswerCall(string callId)
    {
        try
        {
            return await _callService.AnswerCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı cevaplama hatası");
            return false;
        }
    }

    public async Task<bool> HangupCall(string callId)
    {
        try
        {
            return await _callService.HangupCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı kapatma hatası");
            return false;
        }
    }
}
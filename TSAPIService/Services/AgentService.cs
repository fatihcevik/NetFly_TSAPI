using TSAPIService.Models;

namespace TSAPIService.Services;

public interface IAgentService
{
    Task<List<Agent>> GetAllAgentsAsync();
    Task<Agent?> GetAgentAsync(string agentId);
    Task<bool> LoginAgentAsync(string agentId, string password);
    Task<bool> LogoutAgentAsync(string agentId);
    Task<bool> SetAgentStateAsync(string agentId, AgentStatus state);
    Task<CallCenterStats> GetStatsAsync();
}

public class AgentService : IAgentService
{
    private readonly ITSAPIClient _tsapiClient;
    private readonly ILogger<AgentService> _logger;

    public AgentService(ITSAPIClient tsapiClient, ILogger<AgentService> logger)
    {
        _tsapiClient = tsapiClient;
        _logger = logger;
    }

    public async Task<List<Agent>> GetAllAgentsAsync()
    {
        try
        {
            return await _tsapiClient.GetAllAgentsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tüm agentlar alınırken hata");
            return new List<Agent>();
        }
    }

    public async Task<Agent?> GetAgentAsync(string agentId)
    {
        try
        {
            return await _tsapiClient.GetAgentAsync(agentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} alınırken hata", agentId);
            return null;
        }
    }

    public async Task<bool> LoginAgentAsync(string agentId, string password)
    {
        try
        {
            _logger.LogInformation("Agent {AgentId} giriş yapıyor", agentId);
            return await _tsapiClient.LoginAgentAsync(agentId, password);
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
            _logger.LogInformation("Agent {AgentId} çıkış yapıyor", agentId);
            return await _tsapiClient.LogoutAgentAsync(agentId);
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
            _logger.LogInformation("Agent {AgentId} durumu {State} olarak değiştiriliyor", agentId, state);
            return await _tsapiClient.SetAgentStateAsync(agentId, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} durumu değiştirilirken hata", agentId);
            return false;
        }
    }

    public async Task<CallCenterStats> GetStatsAsync()
    {
        try
        {
            return await _tsapiClient.GetStatsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstatistikler alınırken hata");
            return new CallCenterStats();
        }
    }
}
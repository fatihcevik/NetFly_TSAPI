using TSAPIService.Models;

namespace TSAPIService.Services;

public interface ICallService
{
    Task<bool> MakeCallAsync(string agentId, string destination);
    Task<bool> AnswerCallAsync(string callId);
    Task<bool> HangupCallAsync(string callId);
    Task<bool> HoldCallAsync(string callId);
    Task<bool> RetrieveCallAsync(string callId);
    Task<bool> TransferCallAsync(string callId, string destination);
    Task<List<Call>> GetActiveCallsAsync();
    Task<Call?> GetCallAsync(string callId);
}

public class CallService : ICallService
{
    private readonly ITSAPIClient _tsapiClient;
    private readonly ILogger<CallService> _logger;

    public CallService(ITSAPIClient tsapiClient, ILogger<CallService> logger)
    {
        _tsapiClient = tsapiClient;
        _logger = logger;
    }

    public async Task<bool> MakeCallAsync(string agentId, string destination)
    {
        try
        {
            _logger.LogInformation("Agent {AgentId} {Destination} numarasını arıyor", agentId, destination);
            return await _tsapiClient.MakeCallAsync(agentId, destination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arama yapılırken hata");
            return false;
        }
    }

    public async Task<bool> AnswerCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("Çağrı {CallId} cevaplanıyor", callId);
            return await _tsapiClient.AnswerCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı cevaplanırken hata");
            return false;
        }
    }

    public async Task<bool> HangupCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("Çağrı {CallId} kapatılıyor", callId);
            return await _tsapiClient.HangupCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı kapatılırken hata");
            return false;
        }
    }

    public async Task<bool> HoldCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("Çağrı {CallId} beklemeye alınıyor", callId);
            return await _tsapiClient.HoldCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı beklemeye alınırken hata");
            return false;
        }
    }

    public async Task<bool> RetrieveCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("Çağrı {CallId} beklemeden alınıyor", callId);
            return await _tsapiClient.RetrieveCallAsync(callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı beklemeden alınırken hata");
            return false;
        }
    }

    public async Task<bool> TransferCallAsync(string callId, string destination)
    {
        try
        {
            _logger.LogInformation("Çağrı {CallId} {Destination} numarasına aktarılıyor", callId, destination);
            return await _tsapiClient.TransferCallAsync(callId, destination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı aktarılırken hata");
            return false;
        }
    }

    public async Task<List<Call>> GetActiveCallsAsync()
    {
        // Bu method gerçek implementasyonda TSAPI'den aktif çağrıları alacak
        return new List<Call>();
    }

    public async Task<Call?> GetCallAsync(string callId)
    {
        // Bu method gerçek implementasyonda TSAPI'den belirli bir çağrıyı alacak
        return null;
    }
}
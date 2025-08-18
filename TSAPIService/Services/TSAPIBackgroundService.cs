using TSAPIService.Models;

namespace TSAPIService.Services;

public class TSAPIBackgroundService : BackgroundService
{
    private readonly ITSAPIClient _tsapiClient;
    private readonly IEventService _eventService;
    private readonly ILogger<TSAPIBackgroundService> _logger;

    public TSAPIBackgroundService(
        ITSAPIClient tsapiClient,
        IEventService eventService,
        ILogger<TSAPIBackgroundService> logger)
    {
        _tsapiClient = tsapiClient;
        _eventService = eventService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TSAPI Background Service başlatılıyor...");

        // TSAPI event handler'larını kaydet
        _tsapiClient.AgentEvent += OnAgentEvent;
        _tsapiClient.CallEvent += OnCallEvent;
        _tsapiClient.SystemEvent += OnSystemEvent;

        // TSAPI bağlantısını kur
        var connected = await _tsapiClient.ConnectAsync();
        if (!connected)
        {
            _logger.LogError("TSAPI bağlantısı kurulamadı");
            return;
        }

        _logger.LogInformation("TSAPI Background Service başlatıldı");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Periyodik işlemler burada yapılabilir
                await Task.Delay(5000, stoppingToken);
                
                // Bağlantı durumunu kontrol et
                if (!_tsapiClient.IsConnected)
                {
                    _logger.LogWarning("TSAPI bağlantısı kesildi, yeniden bağlanmaya çalışılıyor...");
                    await _tsapiClient.ConnectAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TSAPI Background Service durduruluyor...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TSAPI Background Service'de hata");
        }
        finally
        {
            await _tsapiClient.DisconnectAsync();
            _logger.LogInformation("TSAPI Background Service durduruldu");
        }
    }

    private void OnAgentEvent(object? sender, TSAPIEvent e)
    {
        _logger.LogInformation("Agent Event: {EventType} - {AgentId}", e.Type, e.AgentId);
        _eventService.AddEvent(e);
    }

    private void OnCallEvent(object? sender, TSAPIEvent e)
    {
        _logger.LogInformation("Call Event: {EventType} - {CallId}", e.Type, e.CallId);
        _eventService.AddEvent(e);
    }

    private void OnSystemEvent(object? sender, TSAPIEvent e)
    {
        _logger.LogInformation("System Event: {EventType}", e.Type);
        _eventService.AddEvent(e);
    }
}
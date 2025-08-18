using TSAPIService.Models;
using System.Collections.Concurrent;

namespace TSAPIService.Services;

public interface IEventService
{
    Task<List<TSAPIEvent>> GetRecentEventsAsync(int count = 100);
    Task<List<TSAPIEvent>> GetEventsByAgentAsync(string agentId, int count = 50);
    Task<List<TSAPIEvent>> GetEventsByTypeAsync(TSAPIEventType eventType, int count = 50);
    void AddEvent(TSAPIEvent tsapiEvent);
    event EventHandler<TSAPIEvent>? EventReceived;
}

public class EventService : IEventService
{
    private readonly ILogger<EventService> _logger;
    private readonly ConcurrentQueue<TSAPIEvent> _events = new();
    private const int MaxEvents = 1000;

    public event EventHandler<TSAPIEvent>? EventReceived;

    public EventService(ILogger<EventService> logger)
    {
        _logger = logger;
    }

    public async Task<List<TSAPIEvent>> GetRecentEventsAsync(int count = 100)
    {
        return _events.TakeLast(count).OrderByDescending(e => e.Timestamp).ToList();
    }

    public async Task<List<TSAPIEvent>> GetEventsByAgentAsync(string agentId, int count = 50)
    {
        return _events
            .Where(e => e.AgentId == agentId)
            .TakeLast(count)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
    }

    public async Task<List<TSAPIEvent>> GetEventsByTypeAsync(TSAPIEventType eventType, int count = 50)
    {
        return _events
            .Where(e => e.Type == eventType)
            .TakeLast(count)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
    }

    public void AddEvent(TSAPIEvent tsapiEvent)
    {
        try
        {
            _events.Enqueue(tsapiEvent);
            
            // Maksimum event sayısını kontrol et
            while (_events.Count > MaxEvents)
            {
                _events.TryDequeue(out _);
            }

            _logger.LogDebug("Event eklendi: {EventType} - {AgentId}", tsapiEvent.Type, tsapiEvent.AgentId);
            
            // Event'i dinleyenlere bildir
            EventReceived?.Invoke(this, tsapiEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event eklenirken hata");
        }
    }
}
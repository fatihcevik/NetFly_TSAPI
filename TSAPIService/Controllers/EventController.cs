using Microsoft.AspNetCore.Mvc;
using TSAPIService.Models;
using TSAPIService.Services;

namespace TSAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventController> _logger;

    public EventController(IEventService eventService, ILogger<EventController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Son eventleri getirir
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<List<TSAPIEvent>>> GetRecentEvents([FromQuery] int count = 100)
    {
        try
        {
            var events = await _eventService.GetRecentEventsAsync(count);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Son eventler alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Belirli bir agentin eventlerini getirir
    /// </summary>
    [HttpGet("agent/{agentId}")]
    public async Task<ActionResult<List<TSAPIEvent>>> GetEventsByAgent(string agentId, [FromQuery] int count = 50)
    {
        try
        {
            var events = await _eventService.GetEventsByAgentAsync(agentId, count);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} eventleri alınırken hata", agentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Belirli bir türdeki eventleri getirir
    /// </summary>
    [HttpGet("type/{eventType}")]
    public async Task<ActionResult<List<TSAPIEvent>>> GetEventsByType(TSAPIEventType eventType, [FromQuery] int count = 50)
    {
        try
        {
            var events = await _eventService.GetEventsByTypeAsync(eventType, count);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event türü {EventType} alınırken hata", eventType);
            return StatusCode(500, "Internal server error");
        }
    }
}
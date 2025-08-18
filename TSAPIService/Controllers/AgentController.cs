using Microsoft.AspNetCore.Mvc;
using TSAPIService.Models;
using TSAPIService.Services;

namespace TSAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(IAgentService agentService, ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm agentları getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Agent>>> GetAllAgents()
    {
        try
        {
            var agents = await _agentService.GetAllAgentsAsync();
            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agentlar alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Belirli bir agenti getirir
    /// </summary>
    [HttpGet("{agentId}")]
    public async Task<ActionResult<Agent>> GetAgent(string agentId)
    {
        try
        {
            var agent = await _agentService.GetAgentAsync(agentId);
            if (agent == null)
            {
                return NotFound($"Agent {agentId} bulunamadı");
            }
            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} alınırken hata", agentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Agent giriş yapar
    /// </summary>
    [HttpPost("{agentId}/login")]
    public async Task<ActionResult<bool>> LoginAgent(string agentId, [FromBody] LoginRequest request)
    {
        try
        {
            var result = await _agentService.LoginAgentAsync(agentId, request.Password);
            if (result)
            {
                return Ok(new { Success = true, Message = "Agent başarıyla giriş yaptı" });
            }
            return BadRequest(new { Success = false, Message = "Agent giriş yapamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} giriş yaparken hata", agentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Agent çıkış yapar
    /// </summary>
    [HttpPost("{agentId}/logout")]
    public async Task<ActionResult<bool>> LogoutAgent(string agentId)
    {
        try
        {
            var result = await _agentService.LogoutAgentAsync(agentId);
            if (result)
            {
                return Ok(new { Success = true, Message = "Agent başarıyla çıkış yaptı" });
            }
            return BadRequest(new { Success = false, Message = "Agent çıkış yapamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} çıkış yaparken hata", agentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Agent durumunu değiştirir
    /// </summary>
    [HttpPut("{agentId}/state")]
    public async Task<ActionResult<bool>> SetAgentState(string agentId, [FromBody] SetStateRequest request)
    {
        try
        {
            var result = await _agentService.SetAgentStateAsync(agentId, request.State);
            if (result)
            {
                return Ok(new { Success = true, Message = $"Agent durumu {request.State} olarak değiştirildi" });
            }
            return BadRequest(new { Success = false, Message = "Agent durumu değiştirilemedi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} durumu değiştirilirken hata", agentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrı merkezi istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<CallCenterStats>> GetStats()
    {
        try
        {
            var stats = await _agentService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstatistikler alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class LoginRequest
{
    public string Password { get; set; } = string.Empty;
}

public class SetStateRequest
{
    public AgentStatus State { get; set; }
}
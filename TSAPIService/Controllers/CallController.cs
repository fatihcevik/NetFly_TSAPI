using Microsoft.AspNetCore.Mvc;
using TSAPIService.Models;
using TSAPIService.Services;

namespace TSAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallController : ControllerBase
{
    private readonly ICallService _callService;
    private readonly ILogger<CallController> _logger;

    public CallController(ICallService callService, ILogger<CallController> logger)
    {
        _callService = callService;
        _logger = logger;
    }

    /// <summary>
    /// Arama yapar
    /// </summary>
    [HttpPost("make")]
    public async Task<ActionResult<bool>> MakeCall([FromBody] MakeCallRequest request)
    {
        try
        {
            var result = await _callService.MakeCallAsync(request.AgentId, request.Destination);
            if (result)
            {
                return Ok(new { Success = true, Message = "Arama başlatıldı" });
            }
            return BadRequest(new { Success = false, Message = "Arama başlatılamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arama yapılırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrıyı cevaplar
    /// </summary>
    [HttpPost("{callId}/answer")]
    public async Task<ActionResult<bool>> AnswerCall(string callId)
    {
        try
        {
            var result = await _callService.AnswerCallAsync(callId);
            if (result)
            {
                return Ok(new { Success = true, Message = "Çağrı cevaplandı" });
            }
            return BadRequest(new { Success = false, Message = "Çağrı cevaplanamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı cevaplanırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrıyı kapatır
    /// </summary>
    [HttpPost("{callId}/hangup")]
    public async Task<ActionResult<bool>> HangupCall(string callId)
    {
        try
        {
            var result = await _callService.HangupCallAsync(callId);
            if (result)
            {
                return Ok(new { Success = true, Message = "Çağrı kapatıldı" });
            }
            return BadRequest(new { Success = false, Message = "Çağrı kapatılamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı kapatılırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrıyı beklemeye alır
    /// </summary>
    [HttpPost("{callId}/hold")]
    public async Task<ActionResult<bool>> HoldCall(string callId)
    {
        try
        {
            var result = await _callService.HoldCallAsync(callId);
            if (result)
            {
                return Ok(new { Success = true, Message = "Çağrı beklemeye alındı" });
            }
            return BadRequest(new { Success = false, Message = "Çağrı beklemeye alınamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı beklemeye alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrıyı beklemeden alır
    /// </summary>
    [HttpPost("{callId}/retrieve")]
    public async Task<ActionResult<bool>> RetrieveCall(string callId)
    {
        try
        {
            var result = await _callService.RetrieveCallAsync(callId);
            if (result)
            {
                return Ok(new { Success = true, Message = "Çağrı beklemeden alındı" });
            }
            return BadRequest(new { Success = false, Message = "Çağrı beklemeden alınamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı beklemeden alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Çağrıyı aktarır
    /// </summary>
    [HttpPost("{callId}/transfer")]
    public async Task<ActionResult<bool>> TransferCall(string callId, [FromBody] TransferCallRequest request)
    {
        try
        {
            var result = await _callService.TransferCallAsync(callId, request.Destination);
            if (result)
            {
                return Ok(new { Success = true, Message = "Çağrı aktarıldı" });
            }
            return BadRequest(new { Success = false, Message = "Çağrı aktarılamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çağrı aktarılırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Aktif çağrıları getirir
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<Call>>> GetActiveCalls()
    {
        try
        {
            var calls = await _callService.GetActiveCallsAsync();
            return Ok(calls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif çağrılar alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class MakeCallRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
}

public class TransferCallRequest
{
    public string Destination { get; set; } = string.Empty;
}
using Microsoft.AspNetCore.Mvc;
using TSAPIService.Models;
using TSAPIService.Services;

namespace TSAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TSAPIController : ControllerBase
{
    private readonly ITSAPIClient _tsapiClient;
    private readonly ILogger<TSAPIController> _logger;

    public TSAPIController(ITSAPIClient tsapiClient, ILogger<TSAPIController> logger)
    {
        _tsapiClient = tsapiClient;
        _logger = logger;
    }

    /// <summary>
    /// TSAPI bağlantı durumunu getirir
    /// </summary>
    [HttpGet("connection")]
    public ActionResult<TSAPIConnectionInfo> GetConnectionInfo()
    {
        try
        {
            var connectionInfo = _tsapiClient.GetConnectionInfo();
            return Ok(connectionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bağlantı bilgisi alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// TSAPI bağlantısını yeniden kurar
    /// </summary>
    [HttpPost("reconnect")]
    public async Task<ActionResult<bool>> Reconnect()
    {
        try
        {
            await _tsapiClient.DisconnectAsync();
            var result = await _tsapiClient.ConnectAsync();
            
            if (result)
            {
                return Ok(new { Success = true, Message = "TSAPI bağlantısı yeniden kuruldu" });
            }
            return BadRequest(new { Success = false, Message = "TSAPI bağlantısı kurulamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TSAPI yeniden bağlanırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// İzlenen cihazları getirir
    /// </summary>
    [HttpGet("monitored-devices")]
    public async Task<ActionResult<List<string>>> GetMonitoredDevices()
    {
        try
        {
            var devices = await _tsapiClient.GetMonitoredDevicesAsync();
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İzlenen cihazlar alınırken hata");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cihaz izlemeye başlar
    /// </summary>
    [HttpPost("monitor/{deviceId}")]
    public async Task<ActionResult<bool>> StartMonitoring(string deviceId)
    {
        try
        {
            var result = await _tsapiClient.StartMonitoringAsync(deviceId);
            if (result)
            {
                return Ok(new { Success = true, Message = $"Cihaz {deviceId} izlemeye alındı" });
            }
            return BadRequest(new { Success = false, Message = $"Cihaz {deviceId} izlemeye alınamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cihaz {DeviceId} izlemeye alınırken hata", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cihaz izlemeyi durdurur
    /// </summary>
    [HttpDelete("monitor/{deviceId}")]
    public async Task<ActionResult<bool>> StopMonitoring(string deviceId)
    {
        try
        {
            var result = await _tsapiClient.StopMonitoringAsync(deviceId);
            if (result)
            {
                return Ok(new { Success = true, Message = $"Cihaz {deviceId} izlemeden çıkarıldı" });
            }
            return BadRequest(new { Success = false, Message = $"Cihaz {deviceId} izlemeden çıkarılamadı" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cihaz {DeviceId} izlemeden çıkarılırken hata", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Sistem sağlık durumunu kontrol eder
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        try
        {
            var connectionInfo = _tsapiClient.GetConnectionInfo();
            return Ok(new
            {
                Status = connectionInfo.IsConnected ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Connection = connectionInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sağlık kontrolü yapılırken hata");
            return StatusCode(500, "Internal server error");
        }
    }
}
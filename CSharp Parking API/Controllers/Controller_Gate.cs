using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v2/gate")]
    [AllowAnonymous] // Gate endpoints might be called by hardware
    public class C_Gate : ControllerBase
    {
        private readonly ILogger<C_Gate> _logger;

        public C_Gate(ILogger<C_Gate> logger)
        {
            _logger = logger;
        }

        // Open the gate (called by hardware after successful payment/verification)
        [HttpPost("open")]
        public IActionResult OpenGate([FromBody] OpenGateRequest request)
        {
            try
            {
                _logger.LogInformation("Gate opened for session {SessionId} at parking lot {ParkingLotId}", 
                    request.SessionId, request.ParkingLotId);

                // In a real implementation, this would send a signal to the hardware
                // For now, we just log it
                return Ok(new
                {
                    success = true,
                    message = "Gate opened",
                    sessionId = request.SessionId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening gate");
                return StatusCode(500, new { success = false, message = "Error opening gate." });
            }
        }

        // Close the gate
        [HttpPost("close")]
        public IActionResult CloseGate([FromBody] CloseGateRequest request)
        {
            try
            {
                _logger.LogInformation("Gate closed for session {SessionId}", request.SessionId);

                // In a real implementation, this would send a signal to the hardware
                return Ok(new
                {
                    success = true,
                    message = "Gate closed",
                    sessionId = request.SessionId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing gate");
                return StatusCode(500, new { success = false, message = "Error closing gate." });
            }
        }

        // Get gate status
        [HttpGet("status")]
        public IActionResult GetGateStatus([FromQuery] Guid parkingLotId)
        {
            try
            {
                // In a real implementation, this would query the hardware
                return Ok(new
                {
                    parkingLotId = parkingLotId,
                    status = "operational", // or "maintenance", "error", etc.
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gate status");
                return StatusCode(500, new { success = false, message = "Error getting gate status." });
            }
        }
    }

    public class OpenGateRequest
    {
        public Guid SessionId { get; set; }
        public Guid ParkingLotId { get; set; }
    }

    public class CloseGateRequest
    {
        public Guid SessionId { get; set; }
    }
}


using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CSharpAPI.Models.M_Reservations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [Route("api/v2/sessions")]
    [ApiController]
    [Authorize] // All session endpoints require authentication
    public class C_Sessions : ControllerBase
    {
        private readonly ISessionsService _sessionsService;
        private readonly ILogger<C_Sessions> _logger;
        
        public C_Sessions(ISessionsService sessionsService, ILogger<C_Sessions> logger)
        {
            _sessionsService = sessionsService;
            _logger = logger;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var id) ? id : null;
            }
        }

        private string? CurrentUsername => User.Identity?.Name;

        private bool IsAdminOrAbove => User.IsInRole("SuperAdmin") || User.IsInRole("ParkingLotAdmin");

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] M_Session session)
        {
            if (session == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Users can only start sessions for themselves (set user field to their username)
            var username = CurrentUsername;
            if (string.IsNullOrEmpty(username)) return Unauthorized();
            
            if (!IsAdminOrAbove)
            {
                session.user = username; // Force ownership to current user
            }
            else if (string.IsNullOrWhiteSpace(session.user))
            {
                // Admin creating session but no user specified - default to current user
                session.user = username;
            }

            var startedSession = await _sessionsService.Start(session);
            return Ok(startedSession);
        }

        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopSession(Guid id)
        {
            try
            {
                // Get session to check ownership
                var username = CurrentUsername;
                if (string.IsNullOrEmpty(username)) return Unauthorized();

                var userSessions = await _sessionsService.GetSessionById(username);
                var session = userSessions.FirstOrDefault(s => s.id == id);
                
                // If not found in user's sessions, check if admin can access it
                if (session == null && !IsAdminOrAbove)
                    return NotFound("Session not found or you don't have permission to stop it.");

                // Users can only stop their own sessions, admins can stop any
                if (!IsAdminOrAbove && session != null && session.user != username)
                    return Forbid();

                var stoppedSession = await _sessionsService.Stop(id);
                return Ok(stoppedSession);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionsById(string id)
        {
            // Users can only view their own sessions, admins can view any user's sessions
            var username = CurrentUsername;
            if (!IsAdminOrAbove && (string.IsNullOrEmpty(username) || id != username))
                return Forbid();

            var sessions = await _sessionsService.GetSessionById(id);
            return Ok(sessions);
        }
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSessions(string user, M_Session.PaymentStatus status)
        {
            // Users can only view their own sessions, admins can view any user's sessions
            var username = CurrentUsername;
            if (!IsAdminOrAbove && (string.IsNullOrEmpty(username) || user != username))
                return Forbid();

            var sessions = await _sessionsService.GetAllSessions(user, status);
            return Ok(sessions);
        }

        // Automatic session start at entrance (license plate recognition)
        [HttpPost("auto-start")]
        [AllowAnonymous] // Called by hardware/license plate recognition system
        public async Task<IActionResult> AutoStartSession([FromBody] AutoStartSessionRequest request)
        {
            try
            {
                // Find vehicle by license plate
                var vehiclesService = HttpContext.RequestServices.GetRequiredService<IVehiclesService>();
                var vehicles = await vehiclesService.GetAllVehicles();
                var vehicle = vehicles.FirstOrDefault(v => v.license_plate == request.LicensePlate);
                
                if (vehicle == null)
                    return NotFound($"Vehicle with license plate '{request.LicensePlate}' not found.");

                // Check if user has sufficient balance (if using balance payment)
                var balanceService = HttpContext.RequestServices.GetRequiredService<IUserBalanceService>();
                var estimatedCost = 5.0m; // Estimated cost for session
                var hasBalance = await balanceService.HasSufficientBalance(vehicle.user_id, estimatedCost);

                // Create session automatically
                var session = new M_Session
                {
                    id = Guid.NewGuid(),
                    user = vehicle.user_id.ToString(),
                    vehicle_id = vehicle.id,
                    license_plate = request.LicensePlate,
                    parking_lot_id = request.ParkingLotId,
                    started = DateTime.UtcNow,
                    status = M_Session.PaymentStatus.Unpaid
                };

                var startedSession = await _sessionsService.Start(session);

                // Open gate (in real implementation, this would trigger hardware)
                // For now, we just log it - in production this would call hardware API
                _logger.LogInformation("Gate opened for session {SessionId} at parking lot {ParkingLotId}", 
                    startedSession.id, request.ParkingLotId);

                return Ok(new
                {
                    success = true,
                    session = startedSession,
                    hasSufficientBalance = hasBalance,
                    message = "Session started automatically. Gate opened."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting automatic session");
                return StatusCode(500, $"Error starting automatic session: {ex.Message}");
            }
        }

        // Get parking history for current user
        [HttpGet("me/history")]
        public async Task<IActionResult> GetMyParkingHistory([FromQuery] int? limit = null)
        {
            var username = CurrentUsername;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            try
            {
                var allSessions = await _sessionsService.GetSessionById(username);
                
                // Order by started time descending (most recent first)
                var history = allSessions
                    .OrderByDescending(s => s.started)
                    .Select(s => new
                    {
                        s.id,
                        s.license_plate,
                        s.parking_lot_id,
                        s.started,
                        s.stopped,
                        s.duration_minutes,
                        s.cost,
                        s.status,
                        isActive = s.stopped == default || s.stopped > DateTime.UtcNow
                    });

                if (limit.HasValue)
                    history = history.Take(limit.Value);

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving parking history.");
            }
        }
    }

    public class AutoStartSessionRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public Guid ParkingLotId { get; set; }
    }

}
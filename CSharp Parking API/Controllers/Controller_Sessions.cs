using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    [Authorize] // All session endpoints require authentication
    public class C_Sessions : ControllerBase
    {
        private readonly ISessionsService _sessionsService;
        public C_Sessions(ISessionsService sessionsService)
        {
            _sessionsService = sessionsService;
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
                    return Forbid("Session not found or you don't have permission to stop it.");

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


    }

}
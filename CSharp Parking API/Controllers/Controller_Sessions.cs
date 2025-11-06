using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Mvc;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class C_Sessions : ControllerBase
    {
        private readonly ISessionsService _sessionsService;
        public C_Sessions(ISessionsService sessionsService)
        {
            _sessionsService = sessionsService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] M_Session session)
        {
            if (session == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var startedSession = await _sessionsService.Start(session);
            return Ok(startedSession);
        }

        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopSession(Guid id)
        {
            try
            {
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
            var sessions = await _sessionsService.GetSessionById(id);
            return Ok(sessions);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSessions(string user, M_Session.PaymentStatus status)
        {
            var sessions = await _sessionsService.GetAllSessions(user, status);
            return Ok(sessions);
        }


    }

}
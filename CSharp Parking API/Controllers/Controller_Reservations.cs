using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Mvc;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class C_Reservations : ControllerBase
    {
        private readonly IReservationsService _reservationService;
        public C_Reservations(IReservationsService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllReservations([FromQuery] int page)
        {
            var reservations = await _reservationService.GetAllReservations();

            int totalItem = reservations.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages.");

            var elements = reservations.Skip(page * 10).Take(10).Select(x => new
            {
                x.id,
                x.user_id,
                x.parking_lot_id,
                x.start_time,
                x.end_time,
                x.status,
                x.created_at,
                x.cost
            });
            var response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                TotalPages = totalPages,
                Reservations = elements
            };

            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] M_Reservations reservation)
        {
            if (reservation == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdReservation = await _reservationService.Create(reservation);
            return Ok(createdReservation);
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            try
            {
                if (id == Guid.Empty) return BadRequest("Invalid reservation ID.");
                await _reservationService.Cancel(id);
                return Ok($"Reservation with id {id} has been cancelled.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid reservation ID.");
            var reservation = await _reservationService.GetById(id);
            if (reservation == null) return NotFound("Reservation not found.");
            return Ok(reservation);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ListReservationsByUser(Guid userId, [FromQuery] Status Status)
        {
            if (userId == Guid.Empty) return BadRequest("Invalid user ID.");
            var reservations = await _reservationService.ListByUser(userId, Status);
            return Ok(reservations);
        }

        [HttpGet("check-availability/parking-lots/{parkingLotId}")]
        public async Task<IActionResult> CheckAvailability(Guid parkingLotId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (parkingLotId == Guid.Empty) return BadRequest("Invalid parking lot ID.");
            if (from >= to) return BadRequest("Invalid time range.");

            var availability = await _reservationService.CheckAvailability(parkingLotId, from, to);
            return Ok(availability);
        }
            
        



    }
}


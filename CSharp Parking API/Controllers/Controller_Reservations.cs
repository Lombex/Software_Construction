using CSharpAPI.Models;
using CSharpAPI.Models.DTOs.Reservations;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CSharpAPI.Models.M_Reservations;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Controllers
{
    [Route("api/v2/reservations")]
    [ApiController]
    [Authorize] // All reservation endpoints require authentication
    public class C_Reservations : ControllerBase
    {
        private readonly IReservationsService _reservationService;
        public C_Reservations(IReservationsService reservationService)
        {
            _reservationService = reservationService;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var id) ? id : null;
            }
        }

        private bool IsAdminOrAbove => User.IsInRole("SuperAdmin") || User.IsInRole("ParkingLotAdmin");

        [HttpGet("all")]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can view all reservations
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
        public async Task<IActionResult> CreateReservation([FromBody] Models.ReservationCreateDto dto)
        {
            if (dto == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.start_time >= dto.end_time) return BadRequest("Invalid time range.");

            var reservation = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = dto.user_id,
                parking_lot_id = dto.parking_lot_id,
                vehicle_id = dto.vehicle_id,
                start_time = dto.start_time,
                end_time = dto.end_time,
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 0f
            };

            var createdReservation = await _reservationService.Create(reservation);
            return CreatedAtAction(nameof(GetReservationById), new { id = createdReservation.id }, createdReservation);
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            try
            {
                if (id == Guid.Empty) return BadRequest("Invalid reservation ID.");

                var reservation = await _reservationService.GetById(id);
                if (reservation == null) return NotFound("Reservation not found.");

                // Users can only cancel their own reservations, admins can cancel any
                if (!IsAdminOrAbove && (CurrentUserId == null || reservation.user_id != CurrentUserId.Value))
                    return Forbid();

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

            // Users can only view their own reservations, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || reservation.user_id != CurrentUserId.Value))
                return Forbid();

            return Ok(reservation);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ListReservationsByUser(Guid userId, [FromQuery] Status Status)
        {
            if (userId == Guid.Empty) return BadRequest("Invalid user ID.");

            // Users can only view their own reservations, admins can view any user's reservations
            if (!IsAdminOrAbove && (CurrentUserId == null || userId != CurrentUserId.Value))
                return Forbid();

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

        // Admin-only: Create reservation for another user
        [HttpPost("admin/create-for-user")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> CreateReservationForUser([FromBody] CreateReservationForUserDto dto)
        {
            if (dto == null) return BadRequest("Request body is required.");
            if (dto.user_id == Guid.Empty || dto.vehicle_id == Guid.Empty || dto.parking_lot_id == Guid.Empty)
                return BadRequest("Invalid identifiers.");
            if (dto.start_time >= dto.end_time) return BadRequest("Invalid time range.");

            var reservation = new M_Reservations
            {
                id = dto.id == Guid.Empty ? Guid.NewGuid() : dto.id,
                user_id = dto.user_id,
                vehicle_id = dto.vehicle_id,
                parking_lot_id = dto.parking_lot_id,
                start_time = dto.start_time,
                end_time = dto.end_time,
                status = dto.status,
                created_at = dto.created_at == default ? DateTime.UtcNow : dto.created_at,
                cost = dto.cost
            };

            var createdReservation = await _reservationService.Create(reservation);
            return Ok(createdReservation);
        }
            




    }
}


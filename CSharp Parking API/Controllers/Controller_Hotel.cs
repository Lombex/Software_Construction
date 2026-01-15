using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [Authorize]
    public class C_Hotel : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<C_Hotel> _logger;

        public C_Hotel(IHotelService hotelService, ILogger<C_Hotel> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        // Get all hotels (admin only)
        [HttpGet]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var hotels = await _hotelService.GetAll();
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hotels");
                return StatusCode(500, "Error retrieving hotels.");
            }
        }

        // Get hotel by ID
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetById(id);
                if (hotel == null)
                    return NotFound();
                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel {HotelId}", id);
                return StatusCode(500, "Error retrieving hotel.");
            }
        }

        // Create hotel (admin only)
        [HttpPost]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Create([FromBody] M_Hotel hotel)
        {
            try
            {
                var created = await _hotelService.Create(hotel);
                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                return StatusCode(500, "Error creating hotel.");
            }
        }

        // Update hotel (admin only)
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Update(Guid id, [FromBody] M_Hotel hotel)
        {
            try
            {
                hotel.id = id;
                var updated = await _hotelService.Update(hotel);
                if (!updated)
                    return NotFound();
                return Ok(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel {HotelId}", id);
                return StatusCode(500, "Error updating hotel.");
            }
        }

        // Delete hotel (super admin only)
        [HttpDelete("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _hotelService.Delete(id);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel {HotelId}", id);
                return StatusCode(500, "Error deleting hotel.");
            }
        }

        // Register hotel guest (admin only)
        [HttpPost("{id}/guests")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> RegisterGuest(Guid id, [FromBody] RegisterGuestRequest request)
        {
            try
            {
                var guest = await _hotelService.RegisterGuest(id, request.UserId, request.CheckIn, request.CheckOut, request.ReservationNumber);
                return Ok(guest);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering guest for hotel {HotelId}", id);
                return StatusCode(500, "Error registering guest.");
            }
        }

        // Check out hotel guest
        [HttpPost("guests/{guestId}/checkout")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> CheckOutGuest(Guid guestId)
        {
            try
            {
                var checkedOut = await _hotelService.CheckOutGuest(guestId);
                if (!checkedOut)
                    return NotFound();
                return Ok(new { message = "Guest checked out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out guest {GuestId}", guestId);
                return StatusCode(500, "Error checking out guest.");
            }
        }

        // Get active guests for a hotel
        [HttpGet("{id}/guests/active")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetActiveGuests(Guid id)
        {
            try
            {
                var guests = await _hotelService.GetActiveGuests(id);
                return Ok(guests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active guests for hotel {HotelId}", id);
                return StatusCode(500, "Error retrieving active guests.");
            }
        }
    }

    public class RegisterGuestRequest
    {
        public Guid UserId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? ReservationNumber { get; set; }
    }
}


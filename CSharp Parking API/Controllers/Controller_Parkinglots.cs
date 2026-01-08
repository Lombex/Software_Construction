using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CSharpAPI.Models;
using CSharpAPI.Database;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/parkinglots")]
    [Authorize] // All parking lot endpoints require authentication
    public class C_Parkinglots : ControllerBase
    {
        private readonly SQLite_Database _context;

        public C_Parkinglots(SQLite_Database context)
        {
            _context = context;
        }

        private bool IsAdminOrAbove => User.IsInRole("SuperAdmin") || User.IsInRole("ParkingLotAdmin");

        // GET: api/parkinglots
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lots = await _context.Parkinglots.ToListAsync();
            return Ok(lots);
        }

        // GET: api/parkinglots/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lot = await _context.Parkinglots.FindAsync(id);
            if (lot == null)
                return NotFound($"Parking lot with id {id} not found.");
            return Ok(lot);
        }

        // POST: api/parkinglots
        [HttpPost]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can create parking lots
        public async Task<IActionResult> Create([FromBody] M_Parkinglots lot)
        {
            if (lot == null || string.IsNullOrWhiteSpace(lot.name) || string.IsNullOrWhiteSpace(lot.location) || lot.coordinates == null)
                return BadRequest("Missing required fields: name, location, or coordinates.");

            lot.id = Guid.NewGuid();
            lot.created_at = DateTime.UtcNow;

            _context.Parkinglots.Add(lot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = lot.id }, lot);
        }

        // PUT: api/parkinglots/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can update parking lots
        public async Task<IActionResult> Update(Guid id, [FromBody] M_Parkinglots lot)
        {
            var existing = await _context.Parkinglots.FindAsync(id);
            if (existing == null)
                return NotFound($"Parking lot with id {id} not found.");

            existing.name = lot.name;
            existing.location = lot.location;
            existing.address = lot.address;
            existing.capacity = lot.capacity;
            existing.reserved = lot.reserved;
            existing.daytarriff = lot.daytarriff;
            existing.coordinates = lot.coordinates;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/parkinglots/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can delete parking lots
        public async Task<IActionResult> Delete(Guid id)
        {
            var lot = await _context.Parkinglots.FindAsync(id);
            if (lot == null)
                return NotFound($"Parking lot with id {id} not found.");

            _context.Parkinglots.Remove(lot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // search nearby parking lots
        [HttpGet("search")]
        public async Task<IActionResult> SearchNearby([FromQuery] float lat, [FromQuery] float lng, [FromQuery] float radius)
        {
            // Bounding box method: 1 degree latitude ≈ 111 km
            float degreeRadius = radius / 111f;
            float minLat = lat - degreeRadius;
            float maxLat = lat + degreeRadius;
            float minLng = lng - degreeRadius;
            float maxLng = lng + degreeRadius;

            var lots = await _context.Parkinglots.ToListAsync();
            var nearbyLots = lots.Where(lot =>
                lot.coordinates != null &&
                lot.coordinates.lat >= minLat && lot.coordinates.lat <= maxLat &&
                lot.coordinates.lng >= minLng && lot.coordinates.lng <= maxLng
            ).ToList();

            return Ok(nearbyLots);
        }
    }
}
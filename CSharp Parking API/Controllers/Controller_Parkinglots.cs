using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Models;
using CSharpAPI.Database;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class C_Parkinglots : ControllerBase
    {
        private readonly SQLite_Database _context;

        public C_Parkinglots(SQLite_Database context)
        {
            _context = context;
        }

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
        public async Task<IActionResult> Create([FromBody] M_Parkinglots lot)
        {
            if (lot == null)
                return BadRequest("Invalid data.");

            lot.id = Guid.NewGuid();
            lot.created_at = DateTime.UtcNow;

            _context.Parkinglots.Add(lot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = lot.id }, lot);
        }

        // PUT: api/parkinglots/{id}
        [HttpPut("{id}")]
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
        public async Task<IActionResult> Delete(Guid id)
        {
            var lot = await _context.Parkinglots.FindAsync(id);
            if (lot == null)
                return NotFound($"Parking lot with id {id} not found.");

            _context.Parkinglots.Remove(lot);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    [Authorize] // All vehicle endpoints require authentication
    public class C_Vehicles : ControllerBase
    {
        private readonly IVehiclesService _vehicleService;
        public C_Vehicles(IVehiclesService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can view all vehicles
        public async Task<IActionResult> GetAllVehicles([FromQuery] int page)
        {
            var vehicles = await _vehicleService.GetAllVehicles();

            if (page < 0) return BadRequest("Page number must be non-negative.");

            int totalItem = vehicles.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages.");

            var elements = vehicles.Skip((page * 10)).Take(10).Select(x => new
            {
                id = x.id,
                user_id = x.user_id,
                license_plate = x.license_plate,
                make = x.make,
                model = x.model,
                color = x.color,
                year = x.year,
                created_at = x.created_at,
            });

            var response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                totalPages = totalPages,
                Users = elements
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleByID(Guid Id)
        {
            var vehicle = await _vehicleService.GetByID(Id);
            if (vehicle == null) return NotFound($"User with id {Id} not found."); 
            return Ok(vehicle);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] M_Vehicles m_vehicles)
        {
            if (m_vehicles == null) return BadRequest("Vehicle data is null.");

            // Additional validation can be added here (e.g., check for duplicate license plates)

            if (string.IsNullOrWhiteSpace(m_vehicles.license_plate) || 
                string.IsNullOrWhiteSpace(m_vehicles.make) || string.IsNullOrWhiteSpace(m_vehicles.model))
            {
                return BadRequest("License plate, make, and model cannot be empty or whitespace.");
            }

            if (m_vehicles.year > DateTime.Now)
            {
                return BadRequest("Year cannot be in the future.");
            }

            if (m_vehicles.license_plate.Length > 10)
            {
                return BadRequest("License plate cannot exceed 10 characters.");
            }

            await _vehicleService.CreateVehicle(m_vehicles);
            return CreatedAtAction(nameof(GetVehicleByID), new { id = m_vehicles.id }, m_vehicles);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] M_Vehicles m_vehicles)
        {
            if (m_vehicles == null) return BadRequest("Invalid user data.");

            var existingVehicle = await _vehicleService.GetByID(id);
            if (existingVehicle == null) return NotFound($"User with id {id} not found.");

            await _vehicleService.UpdateVehicle(id, m_vehicles);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var existingVehicle = await _vehicleService.GetByID(id);
            if (existingVehicle == null) return NotFound($"User with id {id} not found.");
            await _vehicleService.DeleteVehicle(id);
            return NoContent();
        }
    }
}
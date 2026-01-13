using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CSharpAPI.Controllers
{
    [Route("api/v2/vehicles")]
    [ApiController]
    [Authorize] // All vehicle endpoints require authentication
    public class C_Vehicles : ControllerBase
    {
        private readonly IVehiclesService _vehicleService;
        public C_Vehicles(IVehiclesService vehicleService)
        {
            _vehicleService = vehicleService;
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
                Vehicles = elements
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleByID(Guid Id)
        {
            var vehicle = await _vehicleService.GetByID(Id);
            if (vehicle == null) return NotFound($"Vehicle with id {Id} not found.");

            // Users can only view their own vehicles, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || vehicle.user_id != CurrentUserId.Value))
                return Forbid();

            return Ok(vehicle);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] M_Vehicles m_vehicles)
        {
            if (m_vehicles == null) return BadRequest("Vehicle data is null.");

            // Users can only create vehicles for themselves, admins can create for any user
            if (!IsAdminOrAbove)
            {
                if (CurrentUserId == null) return Unauthorized();
                m_vehicles.user_id = CurrentUserId.Value; // Force ownership to current user
            }
            else if (m_vehicles.user_id == Guid.Empty)
            {
                // Admin creating vehicle but no user_id specified - default to current user
                if (CurrentUserId == null) return Unauthorized();
                m_vehicles.user_id = CurrentUserId.Value;
            }

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
            if (m_vehicles == null) return BadRequest("Invalid vehicle data.");

            var existingVehicle = await _vehicleService.GetByID(id);
            if (existingVehicle == null) return NotFound($"Vehicle with id {id} not found.");

            // Users can only update their own vehicles, admins can update any
            if (!IsAdminOrAbove && (CurrentUserId == null || existingVehicle.user_id != CurrentUserId.Value))
                return Forbid();

            // Prevent users from changing ownership
            if (!IsAdminOrAbove)
            {
                m_vehicles.user_id = existingVehicle.user_id;
            }

            await _vehicleService.UpdateVehicle(id, m_vehicles);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var existingVehicle = await _vehicleService.GetByID(id);
            if (existingVehicle == null) return NotFound($"Vehicle with id {id} not found.");

            // Users can only delete their own vehicles, admins can delete any
            if (!IsAdminOrAbove && (CurrentUserId == null || existingVehicle.user_id != CurrentUserId.Value))
                return Forbid();

            await _vehicleService.DeleteVehicle(id);
            return NoContent();
        }
    }
}
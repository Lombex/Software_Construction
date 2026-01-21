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

        [HttpGet("my-vehicles")]
        public async Task<IActionResult> GetMyVehicles()
        {
            if (CurrentUserId == null) return Unauthorized();

            var vehicles = await _vehicleService.GetVehiclesByUserId(CurrentUserId.Value);
            
            var response = vehicles.Select(x => new
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

            return Ok(response);
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
                Vehicles = elements
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleByID(Guid id)
        {
            var vehicle = await _vehicleService.GetByID(id);
            if (vehicle == null) return NotFound($"Vehicle with id {id} not found.");
        
            // Users can only view their own vehicles, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || vehicle.user_id != CurrentUserId.Value))
                return Forbid();
        
            return Ok(vehicle);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            if (dto == null) return BadRequest("Vehicle data is null.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Users can only create vehicles for themselves, admins can create for any user
            Guid userId;
            if (!IsAdminOrAbove)
            {
                if (CurrentUserId == null) return Unauthorized();
                userId = CurrentUserId.Value; // Force ownership to current user
            }
            else if (dto.user_id == null || dto.user_id == Guid.Empty)
            {
                // Admin creating vehicle but no user_id specified - default to current user
                if (CurrentUserId == null) return Unauthorized();
                userId = CurrentUserId.Value;
            }
            else
            {
                userId = dto.user_id.Value;
            }

            if (string.IsNullOrWhiteSpace(dto.license_plate) || 
                string.IsNullOrWhiteSpace(dto.make) || string.IsNullOrWhiteSpace(dto.model))
            {
                return BadRequest("License plate, make, and model cannot be empty or whitespace.");
            }

            if (dto.year > DateTime.Now)
            {
                return BadRequest("Year cannot be in the future.");
            }

            if (dto.license_plate.Length > 10)
            {
                return BadRequest("License plate cannot exceed 10 characters.");
            }

            // Map DTO to model
            var vehicle = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = dto.license_plate,
                make = dto.make,
                model = dto.model,
                color = dto.color,
                year = dto.year,
                created_at = DateTime.UtcNow
            };

            try
            {
                await _vehicleService.CreateVehicle(vehicle);
                return CreatedAtAction(nameof(GetVehicleByID), new { id = vehicle.id }, vehicle);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleDto dto)
        {
            if (dto == null) return BadRequest("Invalid vehicle data.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingVehicle = await _vehicleService.GetByID(id);
            if (existingVehicle == null) return NotFound($"Vehicle with id {id} not found.");

            // Users can only update their own vehicles, admins can update any
            if (!IsAdminOrAbove && (CurrentUserId == null || existingVehicle.user_id != CurrentUserId.Value))
                return Forbid();

            // Validate year if provided
            if (dto.year.HasValue && dto.year.Value > DateTime.Now)
            {
                return BadRequest("Year cannot be in the future.");
            }

            // Validate license plate length if provided
            if (dto.license_plate != null && dto.license_plate.Length > 10)
            {
                return BadRequest("License plate cannot exceed 10 characters.");
            }

            // Map DTO to model - only include fields that are provided
            var updatedVehicle = new M_Vehicles
            {
                id = existingVehicle.id,
                user_id = existingVehicle.user_id, // Keep the original user_id, never change it on update
                license_plate = dto.license_plate ?? existingVehicle.license_plate,
                make = dto.make ?? existingVehicle.make,
                model = dto.model ?? existingVehicle.model,
                color = dto.color ?? existingVehicle.color,
                year = dto.year ?? existingVehicle.year,
                created_at = existingVehicle.created_at
            };

            try
            {
                await _vehicleService.UpdateVehicle(id, updatedVehicle);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
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
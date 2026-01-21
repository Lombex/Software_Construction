using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Controllers
{
    [Route("api/v2/profile")]
    [ApiController]
    [Authorize] // All profile endpoints require authentication
    public class C_Profile : ControllerBase
    {
        private readonly IProfileService _profileService;
        public C_Profile(IProfileService profileService)
        {
            _profileService = profileService;
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
        
        [HttpGet()]
        public async Task<IActionResult> GetProfile([FromQuery] Guid id)
        {
            // Users can only view their own profile, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || id != CurrentUserId.Value)) return Forbid();

            var user = await _profileService.GetById(id);
            return Ok(new {
                id = user.id,
                username = user.username,
                name = user.name,
                email = user.email,
                phone = user.phone,
                role = user.role,
                parking_lot_id = user.parking_lot_id,
                created_at = user.created_at,
                birth_year = user.birth_year,
                active = user.active
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            // Users can only view their own profile, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || id != CurrentUserId.Value))
                return Forbid();

            var user = await _profileService.GetById(id);
            return Ok(user);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id)
        {
            // Users can only update their own profile, admins can update any
            if (!IsAdminOrAbove && (CurrentUserId == null || id != CurrentUserId.Value))
                return Forbid();

            var updatedUser = await _profileService.UpdateProfile(id);
            return Ok(updatedUser);
        }

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] string newPassword)
        {
            // Users can only change their own password, admins can change any
            if (!IsAdminOrAbove && (CurrentUserId == null || id != CurrentUserId.Value))
                return Forbid();

            var updatedUser = await _profileService.ChangePassword(id, newPassword);
            return Ok(updatedUser);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProfile(Guid id)
        {
            // Users can only delete their own profile, admins can delete any
            if (!IsAdminOrAbove && (CurrentUserId == null || id != CurrentUserId.Value))
                return Forbid();

            var result = await _profileService.DeleteProfile(id);
            return Ok(new { Success = result });
        }

    }
        
}
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class C_Profile : ControllerBase
    {
        private readonly IProfileService _profileService;
        public C_Profile(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetProfile([FromQuery] Guid id)
        {
            var user = await _profileService.GetById(id);
            return Ok(user);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id)
        {
            var updatedUser = await _profileService.UpdateProfile(id);
            return Ok(updatedUser);
        }

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] string newPassword)
        {
            var updatedUser = await _profileService.ChangePassword(id, newPassword);
            return Ok(updatedUser);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProfile(Guid id)
        {
            var result = await _profileService.DeleteProfile(id);
            return Ok(new { Success = result });
        }

    }
        
}
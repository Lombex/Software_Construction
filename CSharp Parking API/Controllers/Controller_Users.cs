using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSharpAPI.Controllers.Utils;

namespace CSharpAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class C_Users : ControllerBase
    {
        private readonly IUsersService _userService;
        public C_Users(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can view all users
        public async Task<IActionResult> GetAllUsers([FromQuery] int page)
        {
            var users = await _userService.GetAllUsers();

            if (page < 0) return BadRequest("Page number must be non-negative.");

            int totalItem = users.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages.");

            var elements = users.Skip((page * 10)).Take(10).Select(x => new
            {
                id = x.id,
                username = x.username,
                password = x.password,
                name = x.name,
                email = x.email,
                phone = x.phone,
                role = x.role,
                created_at = x.created_at,
                birth_year = x.birth_year,
                active = x.active
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
        public async Task<IActionResult> GetUserByID(Guid Id)
        {
            var user = await _userService.getByID(Id);
            if (user == null) return NotFound($"User with id {Id} not found."); 
            return Ok(user);
        }

        [HttpPost("create")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can create users
        public async Task<IActionResult> CreateUser([FromBody] M_Users m_users)
        {
            if (m_users == null) return BadRequest("User data is null.");

            if (string.IsNullOrEmpty(m_users.username) || string.IsNullOrEmpty(m_users.password) ||
                string.IsNullOrEmpty(m_users.name) || string.IsNullOrEmpty(m_users.email) || string.IsNullOrEmpty(m_users.phone))
            {
                return BadRequest("One or more required fields are missing.");
            }

            if (!C_Utils.IsValidEmail(m_users.email)) return BadRequest("Invalid email format.");

            if (m_users.birth_year > DateTime.Now) return BadRequest("Birth year cannot be in the future.");
            if (!Enum.IsDefined(typeof(M_Users.UserRole), m_users.role)) return BadRequest("Invalid user role.");

            await _userService.CreateUser(m_users);
            return CreatedAtAction(nameof(GetUserByID), new { id = m_users.id }, m_users);
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can update users
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] M_Users m_users)
        {
            if (m_users == null) return BadRequest("Invalid user data.");

            var existingUser = await _userService.getByID(id);
            if (existingUser == null) return NotFound($"User with id {id} not found.");

            await _userService.UpdateProfile(id, m_users);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can delete users
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var existingUser = await _userService.getByID(id);
            if (existingUser == null) return NotFound($"User with id {id} not found.");
            await _userService.DeleteUser(id);
            return NoContent();
        }
    }
}

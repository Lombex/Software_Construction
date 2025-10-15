using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class Controller_Users : ControllerBase
    {
        private readonly IUsersService _userService;
        public Controller_Users(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page)
        {
            var users = await _userService.GetAllUsers();

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
        public async Task<IActionResult> CreateUser([FromBody] M_Users m_users)
        {
            if (m_users == null) return BadRequest("User data is null.");
            await _userService.CreateUser(m_users);
            return CreatedAtAction(nameof(GetUserByID), new { id = m_users.id }, m_users);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] M_Users m_users)
        {
            if (m_users == null) return BadRequest("Invalid user data.");

            var existingUser = await _userService.getByID(id);
            if (existingUser == null) return NotFound($"User with id {id} not found.");

            await _userService.UpdateProfile(id, m_users);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var existingUser = await _userService.getByID(id);
            if (existingUser == null) return NotFound($"User with id {id} not found.");
            await _userService.DeleteUser(id);
            return NoContent();
        }
    }
}

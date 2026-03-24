using GeoTracker.Api.Data;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public class CreateUserRequest
        {
            [Required]
            [MaxLength(50)]
            public string Username { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class UpdateUserRequest
        {
            [Required]
            [MaxLength(50)]
            public string Username { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }
        
        // Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var result = await _context.Users.ToListAsync();
            return Ok(result);            
        }

        // Get user by id
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) return NotFound();
            return Ok(tar);            
        }

        // Create new user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
        {
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            // return the newly created user's info
            return CreatedAtAction(nameof(GetUser), new {id = newUser.Id}, newUser);
        }
        
        // Update user by id
        [HttpPatch("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, UpdateUserRequest request) 
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) 
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                tar.Username = request.Username;
            }
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                tar.Password = request.Password;
            }
            
            await _context.SaveChangesAsync();
            return Ok(tar);

        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) 
            {
                return NotFound();
            }

            _context.Users.Remove(tar);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
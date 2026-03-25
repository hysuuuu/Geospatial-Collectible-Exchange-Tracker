using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.Users;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        
        // Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var response = await _context.Users
                .Select(user => new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                })
                .ToListAsync();

            return Ok(response);
        }

        // Get user by id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
            }

            var res = ToUserResponse(user);
            return Ok(res);            
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
            return CreatedAtAction(nameof(GetUserById), new {id = newUser.Id}, newUser);
        }
        
        // Update user by id
        [HttpPatch("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, UpdateUserRequest request) 
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) 
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
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

            var response = ToUserResponse(tar);
            return Ok(response);

        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) 
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
            }

            _context.Users.Remove(tar);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        private ApiErrorResponse ErrorResponse(int statusCode, string error, string message)
        {
            return ApiErrorFactory.Create(statusCode, error, message, HttpContext?.Request?.Path.Value);
        }
    }
}
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.Users;
using GeoTracker.Api.Models;
using GeoTracker.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        
        // Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var response = await _userRepo.GetAllAsync(user => new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            });

            return Ok(response);
        }

        // Get user by id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
            }

            var res = ToUserResponse(user);
            return Ok(res);
        }

        // Create new user
        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
        {
            var newUser = await _userRepo.CreateAsync(request);
            var response = ToUserResponse(newUser);
            // return the newly created user's info
            return CreatedAtAction(nameof(GetUserById), new {id = newUser.Id}, response);
        }
        
        // Update user by id
        [HttpPatch("{id}")]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, UpdateUserRequest request) 
        {
            var tar = await _userRepo.GetByIdAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
            }
            
            var isUpdated = await _userRepo.UpdateAsync(tar, request);            
            if (!isUpdated) 
            {
                return StatusCode(500, ErrorResponse(500, "Internal Server Error", "Failed to delete user."));
            }
            
            var response = ToUserResponse(tar);
            return Ok(response);
        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tar = await _userRepo.GetByIdAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {id} not found."));
            }

            bool isDeleted = await _userRepo.DeleteAsync(tar);
            if (!isDeleted)
            {
                return StatusCode(500, ErrorResponse(500, "Internal Server Error", "Failed to delete user."));
            }

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
    }
}
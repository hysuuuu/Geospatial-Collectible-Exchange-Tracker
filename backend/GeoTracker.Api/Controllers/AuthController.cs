using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Auth;
using GeoTracker.Api.DTOs.Users;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Models;
using GeoTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class AuthController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // User login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request) 
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(ErrorResponse(401, "Unauthorized", "Invalid email or password."));
            }

            string token = _jwtService.GenerateToken(user);
            var response = new LoginResponse
            {
                Token = token,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            };
            return Ok(response);            
        }
    }    
}
using GeoTracker.Api.Data;
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
        public async Task<ActionResult<User>> CreateUser(User newUser)
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            // return the newly created user's info
            return CreatedAtAction(nameof(GetUser), new {id = newUser.Id}, newUser);
        }

        // Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tar = await _context.Users.FindAsync(id);
            if (tar == null) return NotFound();

            _context.Users.Remove(tar);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
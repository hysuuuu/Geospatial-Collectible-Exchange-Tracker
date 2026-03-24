using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.UserInventory;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UserInventoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserInventoryController(AppDbContext context)
        {
            _context = context;
        }

        // Get all inventory rows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInventory>>> GetInventory()
        {
            var result = await _context.UserInventories
                .Include(ui => ui.User)
                .Include(ui => ui.Collectible)
                .ToListAsync();

            return Ok(result);
        }

        // Get inventory row by id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInventory>> GetInventoryById(int id)
        {
            var inventory = await _context.UserInventories
                .Include(ui => ui.User)
                .Include(ui => ui.Collectible)
                .FirstOrDefaultAsync(ui => ui.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            return Ok(inventory);
        }

        // Get all inventory rows for a user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserInventory>>> GetInventoryByUserId(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"User {userId} not found.");
            }

            var result = await _context.UserInventories
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Collectible)
                .ToListAsync();

            return Ok(result);
        }

        // Add collectible to user inventory.
        // If row already exists for user+collectible, quantity is incremented.
        [HttpPost]
        public async Task<ActionResult<UserInventory>> CreateInventory(CreateUserInventoryRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than 0.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return BadRequest($"User {request.UserId} does not exist.");
            }

            var collectibleExists = await _context.Collectibles.AnyAsync(c => c.Id == request.CollectibleId);
            if (!collectibleExists)
            {
                return BadRequest($"Collectible {request.CollectibleId} does not exist.");
            }

            var existing = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == request.UserId && ui.CollectibleId == request.CollectibleId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.GetAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(existing);
            }

            var newInventory = new UserInventory
            {
                UserId = request.UserId,
                CollectibleId = request.CollectibleId,
                Quantity = request.Quantity,
                GetAt = DateTime.UtcNow
            };

            _context.UserInventories.Add(newInventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventoryById), new { id = newInventory.Id }, newInventory);
        }

        // Update quantity by inventory id
        [HttpPut("{id}")]
        public async Task<ActionResult<UserInventory>> UpdateInventoryQuantity(int id, UpdateUserInventoryQuantityRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than 0.");
            }

            var inventory = await _context.UserInventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            inventory.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(inventory);
        }

        // Delete inventory row by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.UserInventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.UserInventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.UserInventory;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventory()
        {
            var result = await _context.UserInventories
                .Select(ui => new UserInventoryResponse
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    Username = ui.User.Username,
                    CollectibleId = ui.CollectibleId,
                    CollectibleName = ui.Collectible.Name,
                    Quantity = ui.Quantity,
                    GetAt = ui.GetAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // Get inventory row by id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInventoryResponse>> GetInventoryById(int id)
        {
            var inventory = await _context.UserInventories
                .Where(ui => ui.Id == id)
                .Select(ui => new UserInventoryResponse
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    Username = ui.User.Username,
                    CollectibleId = ui.CollectibleId,
                    CollectibleName = ui.Collectible.Name,
                    Quantity = ui.Quantity,
                    GetAt = ui.GetAt
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {id} not found."));
            }

            return Ok(inventory);
        }

        // Get all inventory rows for a user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventoryByUserId(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {userId} not found."));
            }

            var result = await _context.UserInventories
                .Where(ui => ui.UserId == userId)
                .Select(ui => new UserInventoryResponse
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    Username = ui.User.Username,
                    CollectibleId = ui.CollectibleId,
                    CollectibleName = ui.Collectible.Name,
                    Quantity = ui.Quantity,
                    GetAt = ui.GetAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // Get all inventory rows for a collectible
        [HttpGet("collectibles/{collectibleId}")]
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventoryByCollectibleId(int collectibleId)
        {
            var collectibleExists = await _context.Collectibles.AnyAsync(c => c.Id == collectibleId);
            if (!collectibleExists)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {collectibleId} not found."));
            }

            var result = await _context.UserInventories
                .Where(ui => ui.CollectibleId == collectibleId)
                .Select(ui => new UserInventoryResponse
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    Username = ui.User.Username,
                    CollectibleId = ui.CollectibleId,
                    CollectibleName = ui.Collectible.Name,
                    Quantity = ui.Quantity,
                    GetAt = ui.GetAt
                })
                .ToListAsync();

            return Ok(result);
        }

        // Add collectible to user inventory.
        // If row already exists for user+collectible, quantity is incremented.
        [HttpPost]
        public async Task<ActionResult<UserInventoryResponse>> CreateInventory(CreateUserInventoryRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", "Quantity must be greater than 0."));
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", $"User {request.UserId} does not exist."));
            }

            var collectibleExists = await _context.Collectibles.AnyAsync(c => c.Id == request.CollectibleId);
            if (!collectibleExists)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", $"Collectible {request.CollectibleId} does not exist."));
            }

            var existing = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == request.UserId && ui.CollectibleId == request.CollectibleId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.GetAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _context.Entry(existing).Reference(ui => ui.User).LoadAsync();
                await _context.Entry(existing).Reference(ui => ui.Collectible).LoadAsync();

                return Ok(ToUserInventoryResponse(existing));
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

            await _context.Entry(newInventory).Reference(ui => ui.User).LoadAsync();
            await _context.Entry(newInventory).Reference(ui => ui.Collectible).LoadAsync();

            return CreatedAtAction(nameof(GetInventoryById), new { id = newInventory.Id }, ToUserInventoryResponse(newInventory));
        }

        // Update quantity by userId and collectibleId
        // [Authorize]
        [HttpPut("{userId}/collectibles/{collectibleId}")]
        public async Task<ActionResult<UserInventoryResponse>> UpdateInventoryQuantity(int userId, int collectibleId, UpdateUserInventoryQuantityRequest request)
        {
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            // {
            //     return Unauthorized(ErrorResponse(400, "Invalid token", "Invalid token."));
            // }

            if (request.Quantity <= 0)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", "Quantity must be greater than 0."));
            }

            var inventory = await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.CollectibleId == collectibleId);
            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {userId} not found."));
            }

            inventory.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            await _context.Entry(inventory).Reference(ui => ui.User).LoadAsync();
            await _context.Entry(inventory).Reference(ui => ui.Collectible).LoadAsync();

            return Ok(ToUserInventoryResponse(inventory));
        }

        // Delete inventory row by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.UserInventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {id} not found."));
            }

            _context.UserInventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static UserInventoryResponse ToUserInventoryResponse(UserInventory inventory)
        {
            return new UserInventoryResponse
            {
                Id = inventory.Id,
                UserId = inventory.UserId,
                Username = inventory.User?.Username ?? string.Empty,
                CollectibleId = inventory.CollectibleId,
                CollectibleName = inventory.Collectible?.Name ?? string.Empty,
                Quantity = inventory.Quantity,
                GetAt = inventory.GetAt
            };
        }

        private ApiErrorResponse ErrorResponse(int statusCode, string error, string message)
        {
            return ApiErrorFactory.Create(statusCode, error, message, HttpContext?.Request?.Path.Value);
        }
    }
}
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.UserInventory;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UserInventoryController : BaseApiController
    {
        private readonly IUserInventoryRepository _repo;

        public UserInventoryController(IUserInventoryRepository repo)
        {
            _repo = repo;
        }

        // Get all inventory rows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventory()
        {
            var inventories = await _repo.GetAllAsync();
            var result = inventories.Select(ui => ToUserInventoryResponse(ui)).ToList();

            return Ok(result);
        }

        // Get inventory row by id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInventoryResponse>> GetInventoryById(int id)
        {
            var inventory = await _repo.GetByIdAsync(id);

            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {id} not found."));
            }

            return Ok(ToUserInventoryResponse(inventory));
        }

        // Get all inventory rows for a user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventoryByUserId(int userId)
        {
            var userExists = await _repo.UserExistsAsync(userId);
            if (!userExists)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"User {userId} not found."));
            }

            var inventories = await _repo.GetByUserIdAsync(userId);
            var result = inventories.Select(ui => ToUserInventoryResponse(ui)).ToList();

            return Ok(result);
        }

        // Get all inventory rows for a collectible
        [HttpGet("collectibles/{collectibleId}")]
        public async Task<ActionResult<IEnumerable<UserInventoryResponse>>> GetInventoryByCollectibleId(int collectibleId)
        {
            var collectibleExists = await _repo.CollectibleExistsAsync(collectibleId);
            if (!collectibleExists)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {collectibleId} not found."));
            }

            var inventories = await _repo.GetByCollectibleIdAsync(collectibleId);
            var result = inventories.Select(ui => ToUserInventoryResponse(ui)).ToList();

            return Ok(result);
        }

        // Add collectible to user inventory.
        // If row already exists for user+collectible, quantity is incremented.
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UserInventoryResponse>> CreateInventory(CreateUserInventoryRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
            if (currentUserId != request.UserId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "You do not have permission to create invetory for this user."));
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", "Quantity must be greater than 0."));
            }

            var userExists = await _repo.UserExistsAsync(request.UserId);
            if (!userExists)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", $"User {request.UserId} does not exist."));
            }

            var collectibleExists = await _repo.CollectibleExistsAsync(request.CollectibleId);
            if (!collectibleExists)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", $"Collectible {request.CollectibleId} does not exist."));
            }

            var newInventory = await _repo.CreateAsync(request);
            
            return CreatedAtAction(nameof(GetInventoryById), new { id = newInventory.Id }, ToUserInventoryResponse(newInventory));
        }

        // Update quantity by userId and collectibleId
        [Authorize]
        [HttpPut("{userId}/collectibles/{collectibleId}")]
        public async Task<ActionResult<UserInventoryResponse>> UpdateInventoryQuantity(int userId, int collectibleId, UpdateUserInventoryQuantityRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
            if (currentUserId != userId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "You do not have permission to update this inventory."));
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(ErrorResponse(400, "Bad Request", "Quantity must be greater than 0."));
            }

            var inventory = await _repo.GetByUserIdAndCollectibleIdAsync(userId, collectibleId);
            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {userId} not found."));
            }

            await _repo.UpdateAsync(inventory, request);

            return Ok(ToUserInventoryResponse(inventory));
        }

        // Delete inventory row by id
        [Authorize]
        [HttpDelete("{inventoryId}")]
        public async Task<IActionResult> DeleteInventory(int inventoryId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
    
            var inventory = await _repo.GetByIdAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Inventory {inventoryId} not found."));
            }
            if (currentUserId != inventory.UserId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "You do not have permission to delete this inventory."));
            }

            await _repo.DeleteAsync(inventory);

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
    }
}
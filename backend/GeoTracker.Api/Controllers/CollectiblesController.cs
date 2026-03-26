using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Collectibles;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Models;
using GeoTracker.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class CollectiblesController : BaseApiController
    {

        private readonly ICollectibleRepository _collectibleRepo;

        public CollectiblesController(ICollectibleRepository collectibleRepo)
        {
            _collectibleRepo = collectibleRepo;
        }

        // Get all collectibles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollectibleResponse>>> GetCollectibles()
        {
            var collectibles = await _collectibleRepo.GetAllAsync();
            
            var response = collectibles.Select(c => ToCollectibleResponse(c)).ToList();
            return Ok(response);
        }

        // Get collectible by id
        [HttpGet("{id}")]
        public async Task<ActionResult<CollectibleResponse>> GetCollectibleById(int id)
        {
            var tar = await _collectibleRepo.GetByIdAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {id} not found."));
            }

            var response = ToCollectibleResponse(tar);            
            return Ok(response);
        }

        // Create collectible
        [HttpPost]
        public async Task<ActionResult<Collectible>> CreateCollectible(CreateCollectibleRequest request)
        {
            var newColl = await _collectibleRepo.CreateAsync(request);
            
            return CreatedAtAction(nameof(GetCollectibleById), new {id = newColl.Id}, newColl);
        }

        // Update collectible by id
        [HttpPut("{id}")]
        public async Task<ActionResult<CollectibleResponse>> UpdateCollectible(int id, UpdateCollectibleRequest request)
        {
            var tar = await _collectibleRepo.GetByIdAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {id} not found."));
            }

            var isUpdated = await _collectibleRepo.UpdateAsync(tar, request);
            if (!isUpdated)
            {
                return StatusCode(500, ErrorResponse(500, "Internal Server Error", "Failed to delete user."));
            }

            var response = ToCollectibleResponse(tar);
            return Ok(response);
        }

        // Delete collectible by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollectible(int id)
        {
            var tar = await _collectibleRepo.GetByIdAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {id} not found."));
            }

            var isDeleted = await _collectibleRepo.DeleteAsync(tar);
            if (!isDeleted) 
            {
                return StatusCode(500, ErrorResponse(500, "Internal Server Error", "Failed to delete user."));
            }

            return NoContent();
        }

        private static CollectibleResponse ToCollectibleResponse(Collectible collectible)
        {
            return new CollectibleResponse
            {
                Id = collectible.Id,
                Name = collectible.Name,
                Latitude = collectible.Latitude,
                Longitude = collectible.Longitude,
                CreatedAt = collectible.CreatedAt
            };
        }
    }    
}
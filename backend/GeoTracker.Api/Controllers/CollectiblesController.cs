using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Collectibles;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class CollectiblesController : BaseApiController
    {
        private readonly AppDbContext _context;

        public CollectiblesController(AppDbContext context)
        {
            _context = context;
        }

        // Get all collectibles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollectibleResponse>>> GetCollectibles()
        {
            var response = await _context.Collectibles
                .Select(c => new CollectibleResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
            
            return Ok(response);
        }

        // Get collectible by id
        [HttpGet("{id}")]
        public async Task<ActionResult<CollectibleResponse>> GetCollectibleById(int id)
        {
            var tar = await _context.Collectibles.FindAsync(id);
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
            var newColl = new Collectible
            {
                Name = request.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            _context.Collectibles.Add(newColl);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCollectibleById), new {id = newColl.Id}, newColl);
        }

        // Update collectible by id
        [HttpPut("{id}")]
        public async Task<ActionResult<CollectibleResponse>> UpdateCollectible(int id, UpdateCollectibleRequest request)
        {
            var tar = await _context.Collectibles.FindAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {id} not found."));
            }

            tar.Name = request.Name;
            tar.Latitude = request.Latitude;
            tar.Longitude = request.Longitude;

            await _context.SaveChangesAsync();

            var response = ToCollectibleResponse(tar);
            return Ok(response);
        }

        // Delete collectible by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollectible(int id)
        {
            var tar = await _context.Collectibles.FindAsync(id);
            if (tar == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Collectible {id} not found."));
            }

            _context.Collectibles.Remove(tar);
            await _context.SaveChangesAsync();
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
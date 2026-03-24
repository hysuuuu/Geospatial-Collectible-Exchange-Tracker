using GeoTracker.Api.Data;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class CollectiblesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CollectiblesController(AppDbContext context)
        {
            _context = context;
        }

        public class CreateCollectibleRequest
        {
            [Required]
            [MaxLength(50)]
            public string Name { get; set; } = string.Empty;

            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }

        public class UpdateCollectibleRequest
        {
            [Required]
            [MaxLength(50)]
            public string Name { get; set; } = string.Empty;

            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }

        // Get all collectibles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Collectible>>> GetCollectibles()
        {
            var result = await _context.Collectibles.ToListAsync();
            return Ok(result);
        }

        // Get collectible by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Collectible>> GetCollectible(int id)
        {
            var tar = await _context.Collectibles.FindAsync(id);
            if (tar == null) return NotFound();
            return Ok(tar);
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
            return CreatedAtAction(nameof(GetCollectible), new {id = newColl.Id}, newColl);
        }

        // Update collectible by id
        [HttpPut("{id}")]
        public async Task<ActionResult<Collectible>> UpdateCollectible(int id, UpdateCollectibleRequest request)
        {
            var tar = await _context.Collectibles.FindAsync(id);
            if (tar == null) return NotFound();

            tar.Name = request.Name;
            tar.Latitude = request.Latitude;
            tar.Longitude = request.Longitude;

            await _context.SaveChangesAsync();
            return Ok(tar);
        }

        // Delete collectible by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollectible(int id)
        {
            var tar = await _context.Collectibles.FindAsync(id);
            if (tar == null) return NotFound();

            _context.Collectibles.Remove(tar);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
    
}
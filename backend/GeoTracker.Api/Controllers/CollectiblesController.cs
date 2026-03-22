using GeoTracker.Api.Data;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<Collectible>> CreateCollectible(Collectible newColl)
        {
            _context.Collectibles.Add(newColl);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCollectible), new {id = newColl.Id}, newColl);
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
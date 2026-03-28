using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Collectibles;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace GeoTracker.Api.Repository
{
    public class CollectibleRepository : ICollectibleRepository
    {
        private readonly AppDbContext _context;
        public CollectibleRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Collectible>> GetAllAsync()
        {
            return _context.Collectibles.ToListAsync();
        }

        public async Task<Collectible?> GetByIdAsync(int id)
        {
            return await _context.Collectibles.FindAsync(id);
        }

        public async Task<Collectible> CreateAsync(CreateCollectibleRequest request)
        {
            var newColl = new Collectible
            {
                Name = request.Name,
                Location = new Point((double)request.Latitude, (double)request.Longitude) { SRID = 4326 }
            };

            await _context.Collectibles.AddAsync(newColl);
            await _context.SaveChangesAsync();

            return newColl;
        }

        public async Task<bool> UpdateAsync(Collectible collectible, UpdateCollectibleRequest request)
        {
            collectible.Name = request.Name;
            collectible.Location = new Point((double)request.Latitude, (double)request.Longitude) { SRID = 4326 };

            var affectedRows = await _context.SaveChangesAsync();            
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(Collectible collectible)
        {
            _context.Collectibles.Remove(collectible);

            var affectedRows = await _context.SaveChangesAsync();            
            return affectedRows > 0;
        }        
    }
}
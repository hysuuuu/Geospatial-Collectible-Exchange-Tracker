using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Collectibles;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

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
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            await _context.Collectibles.AddAsync(newColl);
            await _context.SaveChangesAsync();

            return newColl;
        }

        public async Task<bool> UpdateAsync(Collectible collectible, UpdateCollectibleRequest request)
        {
            collectible.Name = request.Name;
            collectible.Latitude = request.Latitude;
            collectible.Longitude = request.Longitude;

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
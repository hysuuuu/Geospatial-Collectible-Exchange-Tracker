using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Models;
using GeoTracker.Api.DTOs.Collectibles;

namespace GeoTracker.Api.Interfaces
{
    public interface ICollectibleRepository
    {
        public Task<List<Collectible>> GetAllAsync();

        public Task<Collectible?> GetByIdAsync(int id);

        public Task<Collectible> CreateAsync(CreateCollectibleRequest request);

        public Task<bool> UpdateAsync(Collectible collectible, UpdateCollectibleRequest request);

        public Task<bool> DeleteAsync(Collectible collectible);
    }
}
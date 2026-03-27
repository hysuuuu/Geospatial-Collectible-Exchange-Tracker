using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Models;
using GeoTracker.Api.DTOs.UserInventory;

namespace GeoTracker.Api.Interfaces
{
    public interface IUserInventoryRepository
    {
        public Task<List<UserInventory>> GetAllAsync();

        public Task<UserInventory?> GetByIdAsync(int id);

        public Task<List<UserInventory>> GetByUserIdAsync(int userId);

        public Task<List<UserInventory>> GetByCollectibleIdAsync(int collectibleId);

        public Task<UserInventory?> GetByUserIdAndCollectibleIdAsync(int userId, int collectibleId);

        public Task<UserInventory> CreateAsync(CreateUserInventoryRequest request);

        public Task<bool> UpdateAsync(UserInventory userInventory, UpdateUserInventoryQuantityRequest request);

        public Task<bool> DeleteAsync(UserInventory userInventory);

        public Task<bool> UserExistsAsync(int userId);

        public Task<bool> CollectibleExistsAsync(int collectibleId);
    }
}

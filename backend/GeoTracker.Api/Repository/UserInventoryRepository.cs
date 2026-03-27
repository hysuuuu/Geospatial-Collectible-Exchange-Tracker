using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.UserInventory;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Repository
{
    public class UserInventoryRepository : IUserInventoryRepository
    {
        private readonly AppDbContext _context;

        public UserInventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserInventory>> GetAllAsync()
        {
            return await _context.UserInventories.ToListAsync();
        }

        public async Task<UserInventory?> GetByIdAsync(int id)
        {
            return await _context.UserInventories.FindAsync(id);
        }

        public async Task<List<UserInventory>> GetByUserIdAsync(int userId)
        {
            return await _context.UserInventories
                .Where(ui => ui.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<UserInventory>> GetByCollectibleIdAsync(int collectibleId)
        {
            return await _context.UserInventories
                .Where(ui => ui.CollectibleId == collectibleId)
                .ToListAsync();
        }

        public async Task<UserInventory?> GetByUserIdAndCollectibleIdAsync(int userId, int collectibleId)
        {
            return await _context.UserInventories
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.CollectibleId == collectibleId);
        }

        public async Task<UserInventory> CreateAsync(CreateUserInventoryRequest request)
        {
            var existing = await GetByUserIdAndCollectibleIdAsync(request.UserId, request.CollectibleId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.GetAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _context.Entry(existing).Reference(ui => ui.User).LoadAsync();
                await _context.Entry(existing).Reference(ui => ui.Collectible).LoadAsync();

                return existing;
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

            return newInventory;
        }

        public async Task<bool> UpdateAsync(UserInventory userInventory, UpdateUserInventoryQuantityRequest request)
        {
            userInventory.Quantity = request.Quantity;
            userInventory.GetAt = DateTime.UtcNow;

            _context.UserInventories.Update(userInventory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(UserInventory userInventory)
        {
            _context.UserInventories.Remove(userInventory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> CollectibleExistsAsync(int collectibleId)
        {
            return await _context.Collectibles.AnyAsync(c => c.Id == collectibleId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.Users;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Repository
{
    public class UserRepository : IUserRepository   
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }       

        public Task<List<User>> GetAllAsync()
        {
            return _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> CreateAsync(CreateUserRequest request)
        {            
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<bool> UpdateAsync(User user, UpdateUserRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                user.Username = request.Username;
            }
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }
    }
}
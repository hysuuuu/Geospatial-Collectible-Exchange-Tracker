using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GeoTracker.Api.Models;
using GeoTracker.Api.DTOs.Users;

namespace GeoTracker.Api.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<T>> GetAllAsync<T>(Expression<Func<User, T>> selector);

        public Task<User?> GetByIdAsync(int id);

        public Task<User> CreateAsync(CreateUserRequest request);

        public Task<bool> UpdateAsync(User user, UpdateUserRequest request);

        public Task<bool> DeleteAsync(User user);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Models;
using GeoTracker.Api.DTOs.ExchangeRequest;

namespace GeoTracker.Api.Interfaces
{
    public interface IExchangeRequestRepository
    {
        public Task<List<ExchangeRequest>> GetAllAsync();

        public Task<ExchangeRequest?> GetByIdAsync(int id);

        public Task<List<ExchangeRequest>> GetByInitIdAsync(int id);

        public Task<List<ExchangeRequest>> GetByReceiveIdAsync(int id);
        
        public Task<List<ExchangeRequest>> GetPendingByReceiveIdAsync(int id);

        public Task<ExchangeRequest> CreateAsync(int id, CreateExchangeRequest request);

        public Task<ExchangeRequest> AcceptAsync(ExchangeRequest ex);
        
        public Task<ExchangeRequest> RejectAsync(ExchangeRequest ex);
        
        public Task<ExchangeRequest> CancelAsync(ExchangeRequest ex);
        
        public Task<bool> DeleteAsync(ExchangeRequest ex);        
    }
}
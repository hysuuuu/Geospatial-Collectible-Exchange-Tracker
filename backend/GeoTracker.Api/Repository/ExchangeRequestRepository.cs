using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Data;
using GeoTracker.Api.DTOs.ExchangeRequest;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Repository
{
    public class ExchangeRequestRepository : IExchangeRequestRepository
    {
        private readonly AppDbContext _context;
        public ExchangeRequestRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<ExchangeRequest>> GetAllAsync()
        {
            return await _context.ExchangeRequests.ToListAsync();
        }

        public async Task<ExchangeRequest?> GetByIdAsync(int id)
        {
            return await _context.ExchangeRequests.FindAsync(id);
        }
        
        public async Task<List<ExchangeRequest>> GetByInitIdAsync(int id)
        {
            return await _context.ExchangeRequests
                .Where(e => e.InitiatorUserId == id)
                .ToListAsync();
        }

        public async Task<List<ExchangeRequest>> GetByReceiveIdAsync(int id) 
        {
            return await _context.ExchangeRequests
                .Where(e => e.ReceiverUserId == id)
                .ToListAsync();    
        }

        public async Task<List<ExchangeRequest>> GetPendingByReceiveIdAsync(int id)
        {
            return await _context.ExchangeRequests
                .Where(e => e.ReceiverUserId == id && e.Status == ExchangeStatus.Pending)
                .ToListAsync();
        }

        public async Task<ExchangeRequest> CreateAsync(int id, CreateExchangeRequest request)
        {
            var newExchange = new ExchangeRequest
            {
                InitiatorUserId = id,
                ReceiverUserId = request.ReceiverUserId,
                OfferedCollectibleId = request.OfferedCollectibleId,
                OfferedQuantity = request.OfferedQuantity
            };

            await _context.ExchangeRequests.AddAsync(newExchange);
            await _context.SaveChangesAsync();

            return newExchange;
        }

        public async Task<ExchangeRequest> AcceptAsync(ExchangeRequest ex) 
        {
            if (ex.Status != ExchangeStatus.Pending)
            {
                throw new InvalidOperationException("Only pending requests can be accepted.");
            }

            ex.Status = ExchangeStatus.Completed;
            ex.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // TODO: update userinventory accordingly

            return ex;
        }

        public async Task<ExchangeRequest> RejectAsync(ExchangeRequest ex)
        {
            if (ex.Status != ExchangeStatus.Pending)
            {
                throw new InvalidOperationException("Only pending requests can be rejected.");
            }

            ex.Status = ExchangeStatus.Rejected;
            ex.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ex;
        }

        public async Task<ExchangeRequest> CancelAsync(ExchangeRequest ex)
        {
            if (ex.Status != ExchangeStatus.Pending)
            {
                throw new InvalidOperationException("Only pending requests can be cancelled.");
            }

            ex.Status = ExchangeStatus.Cancelled;
            ex.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ex;
        }

        public async Task<bool> DeleteAsync(ExchangeRequest ex) 
        {
            _context.ExchangeRequests.Remove(ex);
            var affectedRow = await _context.SaveChangesAsync();

            return affectedRow > 0;
        }

    }
}
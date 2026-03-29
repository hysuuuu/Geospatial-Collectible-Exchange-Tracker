using System.ComponentModel.DataAnnotations;
using GeoTracker.Api.Models;

namespace GeoTracker.Api.DTOs.ExchangeRequest
{
    public class ExchangeResponse
    {
        public int Id { get; set; }

        public int InitiatorUserId { get; set; }  
        public int ReceiverUserId { get; set; }   

        public int OfferedCollectibleId { get; set; } 
        public int OfferedQuantity { get; set; }

        public ExchangeStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime? ResolvedAt { get; set; }
    }
}
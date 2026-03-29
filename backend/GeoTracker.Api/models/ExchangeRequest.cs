using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GeoTracker.Api.Models
{
    public class ExchangeRequest
    {
        public int Id { get; set; }

        public int InitiatorUserId { get; set; }  
        public int ReceiverUserId { get; set; }   

        public int OfferedCollectibleId { get; set; } 
        public int OfferedQuantity { get; set; }

        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
}
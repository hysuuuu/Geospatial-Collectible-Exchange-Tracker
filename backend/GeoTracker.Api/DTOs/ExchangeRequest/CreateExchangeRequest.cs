using System.ComponentModel.DataAnnotations;

namespace GeoTracker.Api.DTOs.ExchangeRequest
{
    public class CreateExchangeRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Receiver ID")]
        public int ReceiverUserId { get; set; }   

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Collectible ID")]
        public int OfferedCollectibleId { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int OfferedQuantity { get; set; } = 1;
    }
}
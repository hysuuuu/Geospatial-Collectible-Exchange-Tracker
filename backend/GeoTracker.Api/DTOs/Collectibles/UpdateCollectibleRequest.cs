using System.ComponentModel.DataAnnotations;

namespace GeoTracker.Api.DTOs.Collectibles
{
    public class UpdateCollectibleRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}

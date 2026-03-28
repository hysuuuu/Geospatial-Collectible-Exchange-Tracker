using System.ComponentModel.DataAnnotations;
namespace GeoTracker.Api.DTOs.Collectibles
{
    public class CreateCollectibleRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}

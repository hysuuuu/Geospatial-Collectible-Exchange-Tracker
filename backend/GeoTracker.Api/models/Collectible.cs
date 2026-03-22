using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoTacker.Api.Models
{
    public class Collectibles
    {
        [key]
        public int Id {get; set;}

        [Required]
        [MaxLength(50)]
        public string Name {get; set;} = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude {get; set;}

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude {get; set;}

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoTracker.Api.Models
{
    public class Collectible
    {
        [Key]
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
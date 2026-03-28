using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace GeoTracker.Api.Models
{
    public class Collectible
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [MaxLength(50)]
        public string Name {get; set;} = string.Empty;

        [Column(TypeName = "geometry(Point, 4326)")]
        public Point Location { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
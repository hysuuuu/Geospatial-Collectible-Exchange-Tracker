using System.ComponentModel.DataAnnotations;

namespace GeoTacker.Api.Models
{
    public class UserInventory
    {
        [key]
        public int Id {get; set;}

        [Required]
        public int UserId {get; set;}

        [Required]
        public int CollectibleId {get; set;}

        public int Quantity {get; set;} = 1;

        public DateTime GetAt {get; set;} = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User {get; set;} = null!;

        [ForeignKey("CollectibleId")]
        public Collectible Collectible {get; set;} = null!;        
    }
}
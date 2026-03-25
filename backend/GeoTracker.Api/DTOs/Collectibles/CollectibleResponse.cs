namespace GeoTracker.Api.DTOs.Collectibles
{
    public class CollectibleResponse
    {
        public int Id {get; set;}
        
        public string Name { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
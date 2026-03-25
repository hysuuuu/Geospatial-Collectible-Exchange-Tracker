namespace GeoTracker.Api.DTOs.UserInventory
{
    public class UserInventoryResponse
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int CollectibleId { get; set; }

        public string CollectibleName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime GetAt { get; set; }
    }
}

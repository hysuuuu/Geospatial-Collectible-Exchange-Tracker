namespace GeoTracker.Api.DTOs.UserInventory
{
    public class CreateUserInventoryRequest
    {
        public int UserId { get; set; }

        public int CollectibleId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}

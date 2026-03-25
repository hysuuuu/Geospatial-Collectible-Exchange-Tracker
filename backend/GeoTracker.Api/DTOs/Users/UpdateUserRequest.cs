using System.ComponentModel.DataAnnotations;

namespace GeoTracker.Api.DTOs.Users
{
    public class UpdateUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // [Required]
        public string Password { get; set; } = string.Empty;
    }
}

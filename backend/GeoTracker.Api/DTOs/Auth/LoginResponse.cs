using GeoTracker.Api.DTOs.Users;

namespace GeoTracker.Api.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = new();
    }
}

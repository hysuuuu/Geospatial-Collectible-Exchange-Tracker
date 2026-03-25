namespace GeoTracker.Api.DTOs.Errors
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }

        public string Error { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Path { get; set; }
    }
}

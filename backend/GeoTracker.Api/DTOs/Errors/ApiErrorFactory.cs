namespace GeoTracker.Api.DTOs.Errors
{
    public static class ApiErrorFactory
    {
        public static ApiErrorResponse Create(int statusCode, string error, string message, string? path)
        {
            return new ApiErrorResponse
            {
                StatusCode = statusCode,
                Error = error,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Path = path
            };
        }
    }
}

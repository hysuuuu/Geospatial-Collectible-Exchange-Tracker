using GeoTracker.Api.DTOs.Errors;
using Microsoft.AspNetCore.Mvc;

namespace GeoTracker.Api.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ApiErrorResponse ErrorResponse(int statusCode, string error, string message)
        {
            return ApiErrorFactory.Create(statusCode, error, message, HttpContext?.Request?.Path.Value);
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeoTracker.Api.Tests.Helpers;

public static class ControllerTestHelper
{
    public static ClaimsPrincipal BuildUser(int? userId = null)
    {
        if (userId is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        return new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())
            ],
            "TestAuth"));
    }

    public static ControllerContext BuildControllerContext(string path = "/api/test")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}

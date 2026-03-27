using FluentAssertions;
using GeoTracker.Api.Controllers;
using GeoTracker.Api.DTOs.Auth;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Models;
using GeoTracker.Api.Services;
using GeoTracker.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeoTracker.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        using var context = TestDbContextFactory.CreateContext();
        var jwtService = new Mock<IJwtService>();
        var controller = new AuthController(context, jwtService.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext("/api/auth/login")
        };

        var result = await controller.Login(new LoginRequest
        {
            Email = "missing@example.com",
            Password = "123456"
        });

        var unauthorized = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var payload = unauthorized.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        payload.StatusCode.Should().Be(401);
        payload.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Users.Add(new User
        {
            Username = "test",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("correct")
        });
        await context.SaveChangesAsync();

        var jwtService = new Mock<IJwtService>();
        var controller = new AuthController(context, jwtService.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext("/api/auth/login")
        };

        var result = await controller.Login(new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong"
        });

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        jwtService.Verify(s => s.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsValid()
    {
        using var context = TestDbContextFactory.CreateContext();
        var user = new User
        {
            Id = 11,
            Username = "tester",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("correct")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var controller = new AuthController(context, jwtService.Object);

        var result = await controller.Login(new LoginRequest
        {
            Email = "test@example.com",
            Password = "correct"
        });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Token.Should().Be("jwt-token");
        response.User.Email.Should().Be("test@example.com");

        jwtService.Verify(s => s.GenerateToken(It.Is<User>(u => u.Email == "test@example.com")), Times.Once);
    }
}

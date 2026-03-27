using System.Security.Claims;
using FluentAssertions;
using GeoTracker.Api.Controllers;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.Users;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using GeoTracker.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeoTracker.Api.Tests.Controllers;

public class UsersControllerTests
{
    private static User BuildUser(int id = 1) => new()
    {
        Id = id,
        Username = $"user-{id}",
        Email = $"u{id}@example.com",
        Password = "hashed"
    };

    private static UsersController BuildController(Mock<IUserRepository> repo, int? authUserId = null)
    {
        var controller = new UsersController(repo.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext("/api/users")
        };
        controller.ControllerContext.HttpContext.User = ControllerTestHelper.BuildUser(authUserId);
        return controller;
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithResponseList()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([BuildUser(1), BuildUser(2)]);
        var controller = BuildController(repo);

        var result = await controller.GetUsers();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<UserResponse>>().Subject;
        payload.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenMissing()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((User?)null);
        var controller = BuildController(repo);

        var result = await controller.GetUserById(3);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserById_ShouldReturnOk_WhenFound()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(BuildUser(4));
        var controller = BuildController(repo);

        var result = await controller.GetUserById(4);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<UserResponse>().Subject;
        payload.Id.Should().Be(4);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo);
        controller.ModelState.AddModelError("Username", "required");

        var result = await controller.CreateUser(new CreateUserRequest());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        repo.Verify(r => r.CreateAsync(It.IsAny<CreateUserRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreatedAtAction_WhenSuccess()
    {
        var request = new CreateUserRequest { Username = "new", Email = "new@example.com", Password = "pw" };
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.CreateAsync(request)).ReturnsAsync(BuildUser(9));
        var controller = BuildController(repo);

        var result = await controller.CreateUser(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(UsersController.GetUserById));
        created.Value.Should().BeOfType<UserResponse>();
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, 1);
        controller.ModelState.AddModelError("Username", "required");

        var result = await controller.UpdateUser(1, new UpdateUserRequest());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnUnauthorized_WhenClaimMissing()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, null);

        var result = await controller.UpdateUser(1, new UpdateUserRequest { Username = "x" });

        var unauthorized = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var payload = unauthorized.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        payload.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnForbidden_WhenUserMismatch()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, 2);

        var result = await controller.UpdateUser(1, new UpdateUserRequest { Username = "x" });

        var forbidden = result.Result.Should().BeOfType<ObjectResult>().Subject;
        forbidden.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenTargetMissing()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateUser(1, new UpdateUserRequest { Username = "x" });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_ShouldReturn500_WhenUpdateFails()
    {
        var user = BuildUser(1);
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        repo.Setup(r => r.UpdateAsync(user, It.IsAny<UpdateUserRequest>())).ReturnsAsync(false);
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateUser(1, new UpdateUserRequest { Username = "x" });

        var status = result.Result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnOk_WhenSuccess()
    {
        var user = BuildUser(1);
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        repo.Setup(r => r.UpdateAsync(user, It.IsAny<UpdateUserRequest>())).ReturnsAsync(true);
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateUser(1, new UpdateUserRequest { Username = "updated" });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserResponse>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, 1);
        controller.ModelState.AddModelError("x", "bad");

        var result = await controller.DeleteUser(1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenClaimMissing()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, null);

        var result = await controller.DeleteUser(1);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().BeOfType<ApiErrorResponse>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnForbidden_WhenUserMismatch()
    {
        var repo = new Mock<IUserRepository>();
        var controller = BuildController(repo, 2);

        var result = await controller.DeleteUser(1);

        var forbidden = result.Should().BeOfType<ObjectResult>().Subject;
        forbidden.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenTargetMissing()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteUser(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturn500_WhenDeleteFails()
    {
        var user = BuildUser(1);
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        repo.Setup(r => r.DeleteAsync(user)).ReturnsAsync(false);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteUser(1);

        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNoContent_WhenDeleted()
    {
        var user = BuildUser(1);
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        repo.Setup(r => r.DeleteAsync(user)).ReturnsAsync(true);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
    }
}

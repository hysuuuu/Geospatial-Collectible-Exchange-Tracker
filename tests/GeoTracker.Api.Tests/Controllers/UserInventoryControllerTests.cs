using FluentAssertions;
using GeoTracker.Api.Controllers;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.DTOs.UserInventory;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using GeoTracker.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeoTracker.Api.Tests.Controllers;

public class UserInventoryControllerTests
{
    private static UserInventory BuildInventory(int id = 1, int userId = 10, int collectibleId = 20) => new()
    {
        Id = id,
        UserId = userId,
        CollectibleId = collectibleId,
        Quantity = 2,
        GetAt = DateTime.UtcNow,
        User = new User { Id = userId, Username = $"user-{userId}", Email = $"u{userId}@example.com", Password = "hashed" },
        Collectible = new Collectible { Id = collectibleId, Name = $"coll-{collectibleId}" }
    };

    private static UserInventoryController BuildController(Mock<IUserInventoryRepository> repo, int? authUserId = null)
    {
        var controller = new UserInventoryController(repo.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext("/api/userinventory")
        };
        controller.ControllerContext.HttpContext.User = ControllerTestHelper.BuildUser(authUserId);
        return controller;
    }

    [Fact]
    public async Task GetInventory_ShouldReturnOkWithMappedList()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([BuildInventory(1), BuildInventory(2)]);
        var controller = BuildController(repo);

        var result = await controller.GetInventory();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<UserInventoryResponse>>().Subject;
        payload.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInventoryById_ShouldReturnNotFound_WhenMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserInventory?)null);
        var controller = BuildController(repo);

        var result = await controller.GetInventoryById(1);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetInventoryById_ShouldReturnOk_WhenFound()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BuildInventory(1));
        var controller = BuildController(repo);

        var result = await controller.GetInventoryById(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserInventoryResponse>();
    }

    [Fact]
    public async Task GetInventoryByUserId_ShouldReturnNotFound_WhenUserMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.UserExistsAsync(10)).ReturnsAsync(false);
        var controller = BuildController(repo);

        var result = await controller.GetInventoryByUserId(10);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetInventoryByUserId_ShouldReturnOk_WhenUserExists()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.UserExistsAsync(10)).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync([BuildInventory(1, 10, 20)]);
        var controller = BuildController(repo);

        var result = await controller.GetInventoryByUserId(10);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<UserInventoryResponse>>().Subject;
        payload.Should().ContainSingle();
    }

    [Fact]
    public async Task GetInventoryByCollectibleId_ShouldReturnNotFound_WhenCollectibleMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.CollectibleExistsAsync(20)).ReturnsAsync(false);
        var controller = BuildController(repo);

        var result = await controller.GetInventoryByCollectibleId(20);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetInventoryByCollectibleId_ShouldReturnOk_WhenCollectibleExists()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.CollectibleExistsAsync(20)).ReturnsAsync(true);
        repo.Setup(r => r.GetByCollectibleIdAsync(20)).ReturnsAsync([BuildInventory(1, 10, 20)]);
        var controller = BuildController(repo);

        var result = await controller.GetInventoryByCollectibleId(20);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<UserInventoryResponse>>().Subject;
        payload.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnUnauthorized_WhenClaimMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, null);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 1 });

        var unauthorized = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().BeOfType<ApiErrorResponse>();
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnForbidden_WhenUserMismatch()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, 2);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 1 });

        var forbidden = result.Result.Should().BeOfType<ObjectResult>().Subject;
        forbidden.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnBadRequest_WhenQuantityInvalid()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, 1);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 0 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnBadRequest_WhenUserNotExists()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.UserExistsAsync(1)).ReturnsAsync(false);
        var controller = BuildController(repo, 1);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 1 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnBadRequest_WhenCollectibleNotExists()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.UserExistsAsync(1)).ReturnsAsync(true);
        repo.Setup(r => r.CollectibleExistsAsync(1)).ReturnsAsync(false);
        var controller = BuildController(repo, 1);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 1 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateInventory_ShouldReturnCreatedAtAction_WhenValid()
    {
        var created = BuildInventory(1, 1, 1);
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.UserExistsAsync(1)).ReturnsAsync(true);
        repo.Setup(r => r.CollectibleExistsAsync(1)).ReturnsAsync(true);
        repo.Setup(r => r.CreateAsync(It.IsAny<CreateUserInventoryRequest>())).ReturnsAsync(created);
        var controller = BuildController(repo, 1);

        var result = await controller.CreateInventory(new CreateUserInventoryRequest { UserId = 1, CollectibleId = 1, Quantity = 2 });

        var createdAt = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.ActionName.Should().Be(nameof(UserInventoryController.GetInventoryById));
        createdAt.Value.Should().BeOfType<UserInventoryResponse>();
    }

    [Fact]
    public async Task UpdateInventoryQuantity_ShouldReturnUnauthorized_WhenClaimMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, null);

        var result = await controller.UpdateInventoryQuantity(1, 1, new UpdateUserInventoryQuantityRequest { Quantity = 1 });

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateInventoryQuantity_ShouldReturnForbidden_WhenUserMismatch()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, 2);

        var result = await controller.UpdateInventoryQuantity(1, 1, new UpdateUserInventoryQuantityRequest { Quantity = 1 });

        var forbidden = result.Result.Should().BeOfType<ObjectResult>().Subject;
        forbidden.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateInventoryQuantity_ShouldReturnBadRequest_WhenQuantityInvalid()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateInventoryQuantity(1, 1, new UpdateUserInventoryQuantityRequest { Quantity = 0 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateInventoryQuantity_ShouldReturnNotFound_WhenInventoryMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByUserIdAndCollectibleIdAsync(1, 1)).ReturnsAsync((UserInventory?)null);
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateInventoryQuantity(1, 1, new UpdateUserInventoryQuantityRequest { Quantity = 2 });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateInventoryQuantity_ShouldReturnOk_WhenUpdated()
    {
        var inventory = BuildInventory(1, 1, 1);
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByUserIdAndCollectibleIdAsync(1, 1)).ReturnsAsync(inventory);
        repo.Setup(r => r.UpdateAsync(inventory, It.IsAny<UpdateUserInventoryQuantityRequest>())).ReturnsAsync(true);
        var controller = BuildController(repo, 1);

        var result = await controller.UpdateInventoryQuantity(1, 1, new UpdateUserInventoryQuantityRequest { Quantity = 5 });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserInventoryResponse>();
    }

    [Fact]
    public async Task DeleteInventory_ShouldReturnUnauthorized_WhenClaimMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        var controller = BuildController(repo, null);

        var result = await controller.DeleteInventory(1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeleteInventory_ShouldReturnNotFound_WhenInventoryMissing()
    {
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserInventory?)null);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteInventory(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteInventory_ShouldReturnForbidden_WhenUserMismatch()
    {
        var inventory = BuildInventory(1, 99, 1);
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inventory);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteInventory(1);

        var forbidden = result.Should().BeOfType<ObjectResult>().Subject;
        forbidden.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteInventory_ShouldReturnNoContent_WhenDeleted()
    {
        var inventory = BuildInventory(1, 1, 1);
        var repo = new Mock<IUserInventoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inventory);
        repo.Setup(r => r.DeleteAsync(inventory)).ReturnsAsync(true);
        var controller = BuildController(repo, 1);

        var result = await controller.DeleteInventory(1);

        result.Should().BeOfType<NoContentResult>();
    }
}

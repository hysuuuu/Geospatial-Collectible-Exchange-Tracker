using FluentAssertions;
using GeoTracker.Api.Controllers;
using GeoTracker.Api.DTOs.Collectibles;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;

namespace GeoTracker.Api.Tests.Controllers;

public class CollectiblesControllerTests
{
    private static Collectible BuildCollectible(int id = 1) => new()
    {
        Id = id,
        Name = $"Coll-{id}",
        Location = new Point(121.0 + id, 25.0 + id) { SRID = 4326 },
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetCollectibles_ShouldReturnOkWithMappedResponse()
    {
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([BuildCollectible(1), BuildCollectible(2)]);
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.GetCollectibles();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<CollectibleResponse>>().Subject;
        payload.Should().HaveCount(2);
        payload.First().Name.Should().Be("Coll-1");
    }

    [Fact]
    public async Task GetCollectibleById_ShouldReturnNotFound_WhenMissing()
    {
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Collectible?)null);
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.GetCollectibleById(10);

        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var payload = notFound.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        payload.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetCollectibleById_ShouldReturnOk_WhenFound()
    {
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(BuildCollectible(7));
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.GetCollectibleById(7);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<CollectibleResponse>().Subject;
        payload.Id.Should().Be(7);
    }

    [Fact]
    public async Task CreateCollectible_ShouldReturnCreatedAtAction()
    {
        var request = new CreateCollectibleRequest { Name = "A", Latitude = 1, Longitude = 2 };
        var created = new Collectible { Id = 100, Name = "A", Location = new Point(2, 1) { SRID = 4326 } };

        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.CreateAsync(request)).ReturnsAsync(created);
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.CreateCollectible(request);

        var createdAt = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.ActionName.Should().Be(nameof(CollectiblesController.GetCollectibleById));
        var payload = createdAt.Value.Should().BeOfType<CollectibleResponse>().Subject;
        payload.Id.Should().Be(100);
        payload.Name.Should().Be("A");
        payload.Latitude.Should().Be(1);
        payload.Longitude.Should().Be(2);
    }

    [Fact]
    public async Task UpdateCollectible_ShouldReturnNotFound_WhenMissing()
    {
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Collectible?)null);
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.UpdateCollectible(2, new UpdateCollectibleRequest { Name = "U" });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateCollectible_ShouldReturn500_WhenUpdateFails()
    {
        var existing = BuildCollectible(2);
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing, It.IsAny<UpdateCollectibleRequest>())).ReturnsAsync(false);

        var controller = new CollectiblesController(repo.Object);

        var result = await controller.UpdateCollectible(2, new UpdateCollectibleRequest { Name = "U" });

        var status = result.Result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateCollectible_ShouldReturnOk_WhenUpdated()
    {
        var existing = BuildCollectible(3);
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing, It.IsAny<UpdateCollectibleRequest>())).ReturnsAsync(true);

        var controller = new CollectiblesController(repo.Object);

        var result = await controller.UpdateCollectible(3, new UpdateCollectibleRequest { Name = "U", Latitude = 2, Longitude = 3 });

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<CollectibleResponse>();
    }

    [Fact]
    public async Task DeleteCollectible_ShouldReturnNotFound_WhenMissing()
    {
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync((Collectible?)null);
        var controller = new CollectiblesController(repo.Object);

        var result = await controller.DeleteCollectible(4);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteCollectible_ShouldReturn500_WhenDeleteFails()
    {
        var existing = BuildCollectible(4);
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(existing);
        repo.Setup(r => r.DeleteAsync(existing)).ReturnsAsync(false);

        var controller = new CollectiblesController(repo.Object);

        var result = await controller.DeleteCollectible(4);

        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteCollectible_ShouldReturnNoContent_WhenDeleted()
    {
        var existing = BuildCollectible(5);
        var repo = new Mock<ICollectibleRepository>();
        repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        repo.Setup(r => r.DeleteAsync(existing)).ReturnsAsync(true);

        var controller = new CollectiblesController(repo.Object);

        var result = await controller.DeleteCollectible(5);

        result.Should().BeOfType<NoContentResult>();
    }
}

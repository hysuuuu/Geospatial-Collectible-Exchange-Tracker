using FluentAssertions;
using GeoTracker.Api.Controllers;
using GeoTracker.Api.DTOs.ExchangeRequest;
using GeoTracker.Api.DTOs.Errors;
using GeoTracker.Api.Interfaces;
using GeoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeoTracker.Api.Tests.Controllers;

public class ExchangeRequestControllerTests
{
    private static ExchangeRequest BuildExchangeRequest(int id = 1, int initiatorId = 10, int receiverId = 20) => new()
    {
        Id = id,
        InitiatorUserId = initiatorId,
        ReceiverUserId = receiverId,
        OfferedCollectibleId = 100,
        OfferedQuantity = 5,
        Status = ExchangeStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task DeleteExchange_ShouldReturnNotFound_WhenExchangeNotFound()
    {
        var repo = new Mock<IExchangeRequestRepository>();
        repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ExchangeRequest?)null);
        var controller = new ExchangeRequestsController(repo.Object);

        var result = await controller.DeleteExchange(99);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var payload = notFound.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        payload.StatusCode.Should().Be(404);
        payload.Error.Should().Be("Not Found");
        payload.Message.Should().Contain("99");
    }

    [Fact]
    public async Task DeleteExchange_ShouldReturn500_WhenDeleteFails()
    {
        var exchange = BuildExchangeRequest(1);
        var repo = new Mock<IExchangeRequestRepository>();
        repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exchange);
        repo.Setup(r => r.DeleteAsync(exchange)).ReturnsAsync(false);
        var controller = new ExchangeRequestsController(repo.Object);

        var result = await controller.DeleteExchange(1);

        var statusCode = result.Should().BeOfType<ObjectResult>().Subject;
        statusCode.StatusCode.Should().Be(500);
        var payload = statusCode.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        payload.StatusCode.Should().Be(500);
        payload.Error.Should().Be("Internal Server Error");
    }

    [Fact]
    public async Task DeleteExchange_ShouldReturnNoContent_WhenSuccessful()
    {
        var exchange = BuildExchangeRequest(5);
        var repo = new Mock<IExchangeRequestRepository>();
        repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(exchange);
        repo.Setup(r => r.DeleteAsync(exchange)).ReturnsAsync(true);
        var controller = new ExchangeRequestsController(repo.Object);

        var result = await controller.DeleteExchange(5);

        result.Should().BeOfType<NoContentResult>();
        repo.Verify(r => r.GetByIdAsync(5), Times.Once);
        repo.Verify(r => r.DeleteAsync(exchange), Times.Once);
    }
}

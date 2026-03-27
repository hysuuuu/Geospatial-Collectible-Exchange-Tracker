using GeoTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using TraceLens.Infrastructure.Persistence;

namespace TraceLens.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
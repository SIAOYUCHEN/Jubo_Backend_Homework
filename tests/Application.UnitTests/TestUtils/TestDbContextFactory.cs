using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.TestUtils;

public static class TestDbContextFactory
{
    /// <summary>
    /// Fresh in-memory ApplicationDbContext, isolated per test via a unique database name.
    /// Used so Application-layer handlers can be exercised through real EF Core query
    /// execution (ToListAsync/FirstOrDefaultAsync) without hand-rolling an async LINQ provider.
    /// </summary>
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

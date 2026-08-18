using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.UnitTests.TestUtils;

public static class MockDbSetFactory
{
    /// <summary>
    /// Builds a DbSet double whose enumeration throws, so a test can assert
    /// a repository method never materializes the query (no ToList/foreach
    /// inside the repository itself).
    /// </summary>
    public static DbSet<T> CreateNonEnumerable<T>(IEnumerable<T> source) where T : class
    {
        var queryable = new NonEnumerableQueryable<T>(source);
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator);

        return mockSet.Object;
    }
}

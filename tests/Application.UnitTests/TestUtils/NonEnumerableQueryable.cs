using System.Collections;
using System.Linq.Expressions;

namespace Application.UnitTests.TestUtils;

/// <summary>
/// IQueryable wrapper that throws if enumerated. Used to prove a repository
/// method returns the query without materializing it (no ToList/ToListAsync
/// called inside the repository itself).
/// </summary>
public class NonEnumerableQueryable<T> : IQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public NonEnumerableQueryable(IEnumerable<T> source)
    {
        _inner = source.AsQueryable();
    }

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => _inner.Provider;

    public IEnumerator<T> GetEnumerator() =>
        throw new InvalidOperationException("Query was enumerated — repository must not materialize the IQueryable.");

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

using System.Collections;
using System.Linq.Expressions;

namespace Lagrange.XocMat.DB;

public sealed class DisposableQuery<T> : IQueryable<T>, IDisposable
{
    private readonly IQueryable<T> query;
    private readonly IDisposable disposable;

    public DisposableQuery(IQueryable<T> query, IDisposable disposable)
    {
        this.query = query;
        this.disposable = disposable;
    }
    public Expression Expression => query.Expression;

    public Type ElementType => query.ElementType;

    public IQueryProvider Provider => query.Provider;

    public void Dispose()
    {
        disposable.Dispose();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return query.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return query.GetEnumerator();
    }
}
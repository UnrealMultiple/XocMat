namespace Lagrange.XocMat.Utility;

public class FallbackAsync<T>
{
    private readonly List<Func<CancellationToken, Task<T?>>> _executors = [];

    internal static FallbackAsync<T> Create()
    {
        return new();
    }

    public FallbackAsync<T> Add(Func<CancellationToken, Task<T?>> executor)
    {
        _executors.Add(executor);
        return this;
    }

    public async Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> @default, CancellationToken token = default)
    {
        foreach (Func<CancellationToken, Task<T?>> executor in _executors)
        {
            T? result = await executor(token);
            if (result != null) return result;
        }
        return await @default(token);
    }
}
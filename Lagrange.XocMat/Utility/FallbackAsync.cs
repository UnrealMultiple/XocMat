
namespace Lagrange.XocMat.Utility;

public class FallbackAsync
{
    private readonly List<Func<CancellationToken, Task<bool>>> _executors = [];

    internal static FallbackAsync Create()
    {
        return new();
    }

    public FallbackAsync Add(Func<CancellationToken, Task<bool>> executor)
    {
        _executors.Add(executor);
        return this;
    }

    public async Task<bool> ExecuteAsync(CancellationToken token = default)
    {
        foreach (Func<CancellationToken, Task<bool>> executor in _executors)
        {
            if (await executor(token)) return true;
        }
        return false;
    }
}
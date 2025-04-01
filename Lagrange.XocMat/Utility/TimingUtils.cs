using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Utility;

public class TimingUtils : IHostedService
{
    private readonly Timer Timer;

    internal static PriorityQueue<Action, long> scheduled = new();
    internal static long TimerCount { get; set; }

    public TimingUtils(ILogger<TimingUtils> logger)
    {
        Timer = new Timer(PostUpdate, null, 0, 1000);
        logger.LogInformation("OneBot Timer Start!");
    }

    private void PostUpdate(object? state)
    {
        ++TimerCount;
        while (scheduled.TryPeek(out var action, out var time))
        {
            if (time > TimerCount)
            {
                break;
            }
            action();
            scheduled.Dequeue();
        }
    }

    public static void Schedule(int interval, Action action)
    {
        void Wrapper()
        {
            action();
            Delayed(interval, Wrapper);
        }

        Delayed(interval, Wrapper);
    }

    internal static void Delayed(int delay, Action action)
    {
        scheduled.Enqueue(action, delay + TimerCount);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Timer.DisposeAsync();
    }
}
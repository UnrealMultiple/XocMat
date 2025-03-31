using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Utility;

public class TimingUtils
{
    private Timer timer;
    public TimingUtils(ILogger<TimingUtils> logger)
    {
        timer = new Timer(PostUpdate, null, 0, 1000);
        logger.LogInformation("OneBot Timer Start!");
    }

    private void PostUpdate(object? state)
    {
        ++Timer;
        while (scheduled.TryPeek(out var action, out var time))
        {
            if (time > Timer)
            {
                break;
            }
            action();
            scheduled.Dequeue();
        }
    }

    internal static PriorityQueue<Action, long> scheduled = new();
    public static long Timer { get; internal set; }

    public static void Schedule(int interval, Action action)
    {
        void Wrapper()
        {
            action();
            Delayed(interval, Wrapper);
        }

        Delayed(interval, Wrapper);
    }

    public static void Delayed(int delay, Action action)
    {
        scheduled.Enqueue(action, delay + Timer);
    }
}
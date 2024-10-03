using Microsoft.Extensions.Hosting;


namespace Lagrange.XocMat;

public class XocMatAPI : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Console.Out.WriteLineAsync("你好");
    }
}

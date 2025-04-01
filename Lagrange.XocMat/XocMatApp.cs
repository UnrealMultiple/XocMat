using Lagrange.XocMat.Command;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Lagrange.XocMat;

public class XocMatApp
{
    public HostApplicationBuilder HostApplicationBuilder = Host.CreateApplicationBuilder();

    private IHost _host = null!;

    public IServiceProvider Services => _host.Services;

    public static readonly XocMatApp Instance = new();

    public void Start()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(HostApplicationBuilder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        HostApplicationBuilder.Logging.AddSerilog(Log.Logger);
        _host = HostApplicationBuilder.ConfigureLagrangeCore()
            .ConfigureOneBot()
            .Build()
            .InitializeMusicSigner();
        _host.Run();
    }
}

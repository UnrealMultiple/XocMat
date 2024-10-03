using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XocMat;

public class XocMatHostAppBuilder
{
    private IServiceCollection Services => _hostAppBuilder.Services;

    private ConfigurationManager Configuration => _hostAppBuilder.Configuration;

    private readonly HostApplicationBuilder _hostAppBuilder;

    public XocMatHostAppBuilder(string[] args)
    {
        _hostAppBuilder = new HostApplicationBuilder(args);
    }

    public void Builder()
    { 
        Services.AddHostedService<XocMatAPI>();
    }
}

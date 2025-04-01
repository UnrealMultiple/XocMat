using Lagrange.XocMat.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.XocMat.Extensions;

public static class HostExtension
{
    public static T InitializeMusicSigner<T>(this T host) where T : IHost
    {
        host.Services.GetRequiredService<MusicSigner>();

        return host;
    }
}
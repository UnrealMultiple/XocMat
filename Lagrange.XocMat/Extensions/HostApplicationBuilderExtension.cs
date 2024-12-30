using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.XocMat.Commands;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Lagrange.XocMat.Extensions;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureLagrangeCore(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<OneBotSigner>() // Signer
            .AddSingleton((services) => // BotConfig
            {
                var configuration = services.GetRequiredService<IConfiguration>();

                return new BotConfig
                {
                    Protocol = configuration["Account:Protocol"] switch
                    {
                        "Windows" => Protocols.Windows,
                        "MacOs" => Protocols.MacOs,
                        _ => Protocols.Linux,
                    },
                    AutoReconnect = configuration.GetValue("Account:AutoReconnect", true),
                    UseIPv6Network = configuration.GetValue("Account:UseIPv6Network", false),
                    GetOptimumServer = configuration.GetValue("Account:GetOptimumServer", true),
                    AutoReLogin = configuration.GetValue("Account:AutoReLogin", true),
                    CustomSignProvider = services.GetRequiredService<OneBotSigner>()
                };
            })
            .AddSingleton((services) => // Device
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                string path = configuration["ConfigPath:DeviceInfo"] ?? "device.json";

                var device = File.Exists(path)
                    ? JsonSerializer.Deserialize<BotDeviceInfo>(File.ReadAllText(path)) ?? BotDeviceInfo.GenerateInfo()
                    : BotDeviceInfo.GenerateInfo();

                string deviceJson = JsonSerializer.Serialize(device);
                File.WriteAllText(path, deviceJson);

                return device;
            })
            .AddSingleton((services) => // Keystore
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                string path = configuration["ConfigPath:Keystore"] ?? "keystore.json";

                return File.Exists(path)
                    ? JsonSerializer.Deserialize<BotKeystore>(File.ReadAllText(path)) ?? new()
                    : new();
            })
            .AddSingleton((services) => services.GetRequiredService<OneBotSigner>().GetAppInfo()) // AppInfo
            .AddSingleton((services) => BotFactory.Create( // BotContext
                services.GetRequiredService<BotConfig>(),
                services.GetRequiredService<BotDeviceInfo>(),
                services.GetRequiredService<BotKeystore>(),
                services.GetRequiredService<BotAppInfo>()
            ))
            .AddHostedService<LoginService>();

        return builder;
    }

    public static HostApplicationBuilder ConfigureOneBot(this HostApplicationBuilder builder)
    {
        builder.Services.AddOptions()
            .AddHostedService<XocMatAPI>()
            .AddSingleton<WebSocketServer>()
            .AddSingleton<TShockReceive>()
            .AddSingleton<TerrariaMsgReceiveHandler>()
            .AddSingleton<CommandManager>()
            .AddSingleton<PluginLoader>()
            .AddSingleton<LoggerFactory>()
            .AddSingleton<MusicSigner>();
        return builder;
    }
}
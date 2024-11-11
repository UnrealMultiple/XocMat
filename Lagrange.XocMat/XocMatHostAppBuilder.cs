using Lagrange.Core.Common;
using Lagrange.Core.Utility.Sign;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lagrange.XocMat.Utility;
using System.Text.Json;
using Lagrange.Core.Common.Interface;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Commands;

namespace Lagrange.XocMat;

public sealed class XocMatHostAppBuilder(string[] args)
{
    private IServiceCollection Services => _hostAppBuilder.Services;

    private ConfigurationManager Configuration => _hostAppBuilder.Configuration;

    private readonly HostApplicationBuilder _hostAppBuilder = new HostApplicationBuilder(args);

    public static XocMatApp App { get; set; } = null!;

    public XocMatHostAppBuilder ConfigureConfiguration(string path, bool optional = false, bool reloadOnChange = false)
    {
        Configuration.AddJsonFile(path, optional, reloadOnChange);
        Configuration.AddEnvironmentVariables(); // use this to configure appsettings.json with environment variables in docker container
        return this;
    }

    public XocMatHostAppBuilder ConfigureBots()
    {
        string keystorePath = Configuration["ConfigPath:Keystore"] ?? "keystore.json";
        string deviceInfoPath = Configuration["ConfigPath:DeviceInfo"] ?? "device.json";

        bool isSuccess = Enum.TryParse<Protocols>(Configuration["Account:Protocol"], out var protocol);
        var config = new BotConfig
        {
            Protocol = isSuccess ? protocol : Protocols.Linux,
            AutoReconnect = bool.Parse(Configuration["Account:AutoReconnect"] ?? "true"),
            UseIPv6Network = bool.Parse(Configuration["Account:UseIPv6Network"] ?? "false"),
            GetOptimumServer = bool.Parse(Configuration["Account:GetOptimumServer"] ?? "true"),
            AutoReLogin = bool.Parse(Configuration["Account:AutoReLogin"] ?? "true"),
        };

        BotKeystore keystore;
        if (!File.Exists(keystorePath))
        {
            keystore = Configuration["Account:Uin"] is { } uin && Configuration["Account:Password"] is { } password
                    ? new BotKeystore(uint.Parse(uin), password)
                    : new BotKeystore();
            string? directoryPath = Path.GetDirectoryName(keystorePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        else
        {
            keystore = JsonSerializer.Deserialize<BotKeystore>(File.ReadAllText(keystorePath)) ?? new BotKeystore();
        }

        BotDeviceInfo deviceInfo;
        if (!File.Exists(deviceInfoPath))
        {
            deviceInfo = BotDeviceInfo.GenerateInfo();
            string json = JsonSerializer.Serialize(deviceInfo);
            string? directoryPath = Path.GetDirectoryName(deviceInfoPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(deviceInfoPath, json);
        }
        else
        {
            deviceInfo = JsonSerializer.Deserialize<BotDeviceInfo>(File.ReadAllText(deviceInfoPath)) ?? BotDeviceInfo.GenerateInfo();
        }

        Services.AddSingleton(BotFactory.Create(config, deviceInfo, keystore));

        return this;
    }

    public XocMatHostAppBuilder ConfigureOneBot()
    {
       
        Services.AddSingleton<SignProvider, OneBotSigner>();
        Services.AddHostedService<XocMatAPI>();
        Services.AddSingleton<WebSocketServer>();
        Services.AddSingleton<TShockReceive>();
        Services.AddSingleton<TerrariaMsgReceiveHandler>();
        Services.AddSingleton<CommandManager>();
        Services.AddSingleton<PluginLoader>();
       
        Services.AddSingleton<MusicSigner>();
        return this;
    }

    public XocMatHostAppBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        Services.AddLogging(configureLogging);
        return this;
    }

    [MemberNotNull(nameof(App))]
    public XocMatApp Build()
    { 
        App = new XocMatApp(_hostAppBuilder.Build());
        return App;
    } 
}

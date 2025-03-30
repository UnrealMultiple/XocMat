using System.Data;
using Lagrange.Core;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Lagrange.XocMat;

public class XocMatAPI : BackgroundService
{
    public static BotContext BotContext { get; private set; } = null!;

    public static IDbConnection DB { get; internal set; } = null!;

    public static string PATH => Environment.CurrentDirectory;

    public static string SAVE_PATH => Path.Combine(PATH, "Config");

    public static SocketAdapter TerrariaMsgReceive => XocMatApp.Instance.Services.GetRequiredService<SocketAdapter>();

    public static CommandManager CommandManager => XocMatApp.Instance.Services.GetRequiredService<CommandManager>();

    public static WebSocketServer WsServer => XocMatApp.Instance.Services.GetRequiredService<WebSocketServer>();

    public static PluginLoader PluginLoader => XocMatApp.Instance.Services.GetRequiredService<PluginLoader>();

    public XocMatAPI(BotContext botContext, ILogger<XocMatAPI> logger)
    {
        BotContext = botContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(SAVE_PATH)) Directory.CreateDirectory(SAVE_PATH);
        string sql = Path.Combine(PATH, XocMatSetting.Instance.DbPath);
        if (Path.GetDirectoryName(sql) is string path)
        {
            Directory.CreateDirectory(path);
            DB = new SqliteConnection(string.Format("Data Source={0}", sql));
        }
        await WsServer.Start(stoppingToken);
    }
}

using System.Data;
using Lagrange.Core;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;


namespace Lagrange.XocMat;

public class XocMatAPI : IHostedService
{
    public static BotContext BotContext { get; private set; } = null!;

    public static IDbConnection DB { get; internal set; } = null!;

    public static string PATH => Environment.CurrentDirectory;

    public static string SAVE_PATH => Path.Combine(PATH, "Config");

    public static SocketAdapter SocketAdapter { get; private set; } = null!;

    public static CommandManager CommandManager { get; private set; } = null!;

    public static WebSocketServer WsServer { get; private set; } = null!;

    public static PluginLoader PluginLoader { get; private set; } = null!;

    public XocMatAPI(BotContext botContext, PluginLoader pluginLoader, CommandManager cmdManager, WebSocketServer wsServer, SocketAdapter socketAdapter)
    {
        BotContext = botContext;
        PluginLoader = pluginLoader;
        WsServer = wsServer;
        CommandManager = cmdManager;
        SocketAdapter = socketAdapter;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        PluginLoader.UnLoad();
        BotContext.Invoker.OnGroupMessageReceived -= CommandManager.GroupCommandAdapter;
        BotContext.Invoker.OnFriendMessageReceived -= CommandManager.FriendCommandAdapter;
        BotContext.Invoker.OnGroupMessageReceived -= SocketAdapter.GroupMessageForwardAdapter;
        WsServer.OnMessage -= SocketAdapter.Adapter;
        await WsServer.StopAsync(cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(SAVE_PATH)) Directory.CreateDirectory(SAVE_PATH);
        string sql = Path.Combine(PATH, XocMatSetting.Instance.DbPath);
        if (Path.GetDirectoryName(sql) is string path)
        {
            Directory.CreateDirectory(path);
            DB = new SqliteConnection(string.Format("Data Source={0}", sql));
        }
        PluginLoader.Load();
        BotContext.Invoker.OnGroupMessageReceived += CommandManager.GroupCommandAdapter;
        BotContext.Invoker.OnFriendMessageReceived += CommandManager.FriendCommandAdapter;
        BotContext.Invoker.OnGroupMessageReceived += SocketAdapter.GroupMessageForwardAdapter;
        WsServer.OnMessage += SocketAdapter.Adapter;
        await WsServer.Start(cancellationToken);
    }
}

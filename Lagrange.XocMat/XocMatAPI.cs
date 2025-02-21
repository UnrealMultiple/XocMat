using System.Data;
using Lagrange.Core;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Plugin;
using Lagrange.XocMat.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;


namespace Lagrange.XocMat;

public class XocMatAPI : BackgroundService
{
    public static BotContext BotContext { get; private set; } = null!;

    public static IDbConnection DB { get; internal set; } = null!;

    public static string PATH => Environment.CurrentDirectory;

    public static string SAVE_PATH => Path.Combine(PATH, "Config");

    public static TerrariaMsgReceiveHandler TerrariaMsgReceive => XocMatApp.Instance.Services.GetRequiredService<TerrariaMsgReceiveHandler>();

    public static CommandManager CommandManager => XocMatApp.Instance.Services.GetRequiredService<CommandManager>();

    public static PluginLoader PluginLoader => XocMatApp.Instance.Services.GetRequiredService<PluginLoader>();

    public XocMatAPI(BotContext botContext, ILogger<XocMatAPI> logger)
    {
        BotContext = botContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(SAVE_PATH))
            Directory.CreateDirectory(SAVE_PATH);
        SystemHelper.KillChrome();
        //初始化数据库
        InitDb();
        return Task.CompletedTask;
    }

    private static void InitDb()
    {
        switch (XocMatSetting.Instance.DbType.ToLower())
        {
            case "sqlite":
                {
                    string sql = Path.Combine(PATH, XocMatSetting.Instance.DbPath);
                    if (Path.GetDirectoryName(sql) is string path)
                    {
                        Directory.CreateDirectory(path);
                        DB = new SqliteConnection(string.Format("Data Source={0}", sql));
                        break;
                    }
                    throw new ArgumentNullException("无法找到数据库路径!");
                }
            case "mysql":
                {
                    DB = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                        XocMatSetting.Instance.DbHost, XocMatSetting.Instance.DbPort, XocMatSetting.Instance.DbName, XocMatSetting.Instance.DbUserName, XocMatSetting.Instance.DbPassword)
                    };
                    break;
                }
            default:
                throw new TypeLoadException("无法使用类型:" + XocMatSetting.Instance.DbType);

        }
    }
}

using Lagrange.Core;
using Lagrange.XocMat.Commands;
using Lagrange.XocMat.Configured;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Utility;
using Markdig;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data;


namespace Lagrange.XocMat;

public class XocMatAPI : BackgroundService
{
    public static BotContext BotContext { get; private set; } = null!;

    public static IDbConnection DB { get; internal set; } = null!;

    public static SignManager SignManager { get; internal set; } = null!;

    public static GroupMananger GroupManager { get; internal set; } = null!;

    public static AccountManager AccountManager { get; internal set; } = null!;

    public static CurrencyManager CurrencyManager { get; internal set; } = null!;

    public static TerrariaUserManager TerrariaUserManager { get; internal set; } = null!;

    public static string PATH => Environment.CurrentDirectory;

    public static string SAVE_PATH => Path.Combine(PATH, "Config");

    internal static string ConfigPath => Path.Combine(SAVE_PATH, "MorMor.Json");

    internal static string UserLocationPath => Path.Combine(SAVE_PATH, "UserLocation.Json");

    internal static string TerrariaShopPath => Path.Combine(SAVE_PATH, "Shop.Json");

    internal static string TerrariaPrizePath => Path.Combine(SAVE_PATH, "Prize.Json");

    public static XocMatSetting Setting { get; internal set; } = new();

    public static UserLocation UserLocation { get; internal set; } = new();

    public static TerrariaShop TerrariaShop { get; internal set; } = new();

    public static TerrariaPrize TerrariaPrize { get; internal set; } = new();

    public static TerrariaMsgReceiveHandler? TerrariaMsgReceive => XocMatHostAppBuilder.App?.Services.GetRequiredService<TerrariaMsgReceiveHandler>();

    public static CommandManager? Command => XocMatHostAppBuilder.App?.Services.GetRequiredService<CommandManager>();

    public XocMatAPI(BotContext botContext, ILogger<XocMatAPI> logger)
    {
        BotContext = botContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(SAVE_PATH))
            Directory.CreateDirectory(SAVE_PATH);
        SystemHelper.KillChrome();
        //读取Config
        LoadConfig();
        //初始化数据库
        InitDb();
        return Task.CompletedTask;
    }


    internal static void LoadConfig()
    {
        Setting = ConfigHelpr.LoadConfig(ConfigPath, Setting);
        UserLocation = ConfigHelpr.LoadConfig(UserLocationPath, UserLocation);
        TerrariaShop = ConfigHelpr.LoadConfig(TerrariaShopPath, TerrariaShop);
        TerrariaPrize = ConfigHelpr.LoadConfig(TerrariaPrizePath, TerrariaPrize);
    }

    internal static void ConfigSave()
    {
        ConfigHelpr.Write(ConfigPath, Setting);
        ConfigHelpr.Write(UserLocationPath, UserLocation);
        ConfigHelpr.Write(TerrariaShopPath, TerrariaShop);
        ConfigHelpr.Write(TerrariaPrizePath, TerrariaPrize);
    }

    private void InitDb()
    {
        switch (Setting.DbType.ToLower())
        {
            case "sqlite":
                {
                    string sql = Path.Combine(PATH, Setting.DbPath);
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
                        Setting.DbHost, Setting.DbPort, Setting.DbName, Setting.DbUserName, Setting.DbPassword)
                    };
                    break;
                }
            default:
                throw new TypeLoadException("无法使用类型:" + Setting.DbType);

        }
        GroupManager = new();
        AccountManager = new();
        CurrencyManager = new();
        SignManager = new();
        TerrariaUserManager = new();
    }
}

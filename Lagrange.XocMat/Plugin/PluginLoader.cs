using System.Reflection;
using Lagrange.Core;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public class PluginLoader(CommandManager cmdManager, BotContext botContext, ILogger<PluginLoader> logger)
{
    public CommandManager CommandManager { get; } = cmdManager;

    public BotContext BotContext { get; } = botContext;

    public ILogger<PluginLoader> Logger { get; } = logger;

    public PluginContext PluginContext { get; private set; } = new(Guid.NewGuid().ToString());

    public readonly string PATH = Path.Combine(XocMatAPI.PATH, "Plugins");

    /// <summary>
    /// 加载插件
    /// </summary>
    public void Load()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsDefined(typeof(ConfigSeries), false))
            {
                MethodInfo method = type.BaseType!.GetMethod("Load") ?? throw new MissingMethodException($"method 'Load()' is missing inside the lazy loaded config class");
                object? name = method.Invoke(null, []);
                Logger.LogInformation("[{Time}] [PluginLoader] Config Registered Successfully: {name}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), name);
            }
        }
        DirectoryInfo directoryInfo = new(PATH);
        if (!directoryInfo.Exists)
            directoryInfo.Create();
        PluginContext.LoadPlugins(directoryInfo, Logger, CommandManager, BotContext);
        CommandManager.RegisterCommand(Assembly.GetExecutingAssembly());
    }

    public void UnLoad()
    {
        PluginContext.UnloadPlugin();
        PluginContext = new(Guid.NewGuid().ToString());
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

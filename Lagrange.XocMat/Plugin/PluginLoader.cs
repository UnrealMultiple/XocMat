using System.Reflection;
using Lagrange.Core;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public class PluginLoader
{
    public CommandManager CommandManager { get; }

    public BotContext BotContext { get; }

    public ILogger<PluginLoader> Logger { get; }

    public PluginLoader(CommandManager cmdManager, BotContext botContext, ILogger<PluginLoader> logger)
    {
        CommandManager = cmdManager;
        BotContext = botContext;
        Logger = logger;
        Load();
    }

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
                Logger.LogInformation($"config registered: {name}");
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

    internal Assembly? Resolve(object? sender, ResolveEventArgs args)
    {
        string dirpath = Path.Combine(XocMatAPI.PATH, "bin");
        if (!Directory.Exists(dirpath))
            Directory.CreateDirectory(dirpath);
        string fileName = args.Name.Split(',')[0];
        string path = Path.Combine(dirpath, fileName + ".dll");
        try
        {
            if (File.Exists(path))
            {
                Assembly assembly;

                assembly = Assembly.Load(File.ReadAllBytes(path));

                return assembly;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                string.Format("Error on resolving assembly \"{0}.dll\":\n{1}", fileName, ex));
        }
        return null; ;
    }


}

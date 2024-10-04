using Lagrange.Core;
using Lagrange.XocMat.Commands;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Lagrange.XocMat.Plugin;

public class PluginLoader(CommandManager cmdManager, BotContext botContext, ILogger<PluginLoader> logger)
{
    public PluginContext PluginContext { get; private set; } = new(Guid.NewGuid().ToString());

    public readonly string PATH = Path.Combine(XocMatAPI.PATH, "Plugins");

    /// <summary>
    /// 加载插件
    /// </summary>
    public void Load()
    {
        DirectoryInfo directoryInfo = new(PATH);
        if (!directoryInfo.Exists)
            directoryInfo.Create();
        PluginContext.LoadPlugins(directoryInfo,logger, cmdManager, botContext);
        cmdManager.MappingCommands(Assembly.GetExecutingAssembly());
        PluginContext.LoadAssemblys.ForEach(cmdManager.MappingCommands);    }

    public void UnLoad()
    {
        PluginContext.UnloadPlugin();
        PluginContext = new(Guid.NewGuid().ToString());
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    internal Assembly? Resolve(object? sender, ResolveEventArgs args)
    {
        var dirpath = Path.Combine(XocMatAPI.PATH, "bin");
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

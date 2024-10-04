using Lagrange.Core;
using Lagrange.XocMat.Commands;
using Lagrange.XocMat.Extensions;
using System.Reflection;
using System.Runtime.Loader;

namespace Lagrange.XocMat.Plugin;

public class PluginContext(string name) : AssemblyLoadContext(name, true)
{
    public readonly List<Assembly> LoadAssemblys = [];

    public readonly List<XocMatPlugin> Plugins = [];

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        foreach (Assembly assembly in LoadAssemblys)
        {
            if (assembly.GetName() == assemblyName)
                return assembly;
        }
        return Default.LoadFromAssemblyName(assemblyName);
    }

    public void LoadPlugins(DirectoryInfo dir, CommandManager cmdManager, BotContext bot)
    {
        foreach (FileInfo file in dir.GetFiles("*.dll", SearchOption.AllDirectories))
        {
            using var stream = file.OpenRead();
            using var pdbStream = File.Exists(Path.ChangeExtension(file.FullName, ".pdb")) ? File.OpenRead(Path.ChangeExtension(file.FullName, ".pdb")) : null;
            var assembly = LoadFromStream(stream, pdbStream);
            LoadAssemblys.Add(assembly);
        }
        foreach (Assembly assembly in LoadAssemblys)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsSubclassOf(typeof(XocMatPlugin)) && !type.IsAbstract)
                {
                    if (Activator.CreateInstance(type, cmdManager, bot) is XocMatPlugin instance)
                        Plugins.Add(instance);
                }
            }
        }
        Plugins.OrderBy(p => p.Order).ForEach(p => p.Initialize());
    }

    public void UnloadPlugin()
    {
        Plugins.ForEach(x => x.Dispose());
        Plugins.Clear();
        LoadAssemblys.Clear();
        Unload();
    }
}

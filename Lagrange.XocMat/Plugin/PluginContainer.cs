
namespace Lagrange.XocMat.Plugin;

public class PluginContainer(XocMatPlugin plugin)
{
    public XocMatPlugin Plugin { get; protected set; } = plugin;

    public bool Initialized { get; protected set; }

    public void DeInitialize()
    {
        Plugin.Dispose();
        Initialized = false;
    }

    public void Initialize()
    { 
        Plugin.Initialize();
        Initialized = true;
    }
}

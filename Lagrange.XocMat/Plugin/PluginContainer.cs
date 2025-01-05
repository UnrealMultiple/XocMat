
namespace Lagrange.XocMat.Plugin;

public class PluginContainer(XocMatPlugin plugin)
{
    public XocMatPlugin Plugin { get; protected set; } = plugin;

    public bool Initialized { get; protected set; }

    public void DeInitialize()
    {
        Initialized = false;
        Plugin.Dispose();
    }

    public void Initialize()
    {
        Initialized = true;
        Plugin.Initialize();
    }
}

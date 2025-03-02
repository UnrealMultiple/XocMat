using Lagrange.Core;
using Lagrange.XocMat.Attributes;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public abstract class XocMatPlugin(ILogger logger, BotContext bot) : IDisposable
{
    public ILogger Logger { get; } = logger;

    public BotContext BotContext { get; } = bot;

    public virtual string Name
    {
        get
        {
            return "Plugin";
        }
    }


    public virtual string Author
    {
        get
        {
            return "None";
        }
    }


    public virtual string Description
    {
        get
        {
            return "None";
        }
    }


    public virtual Version Version
    {
        get
        {
            return new Version(1, 0);
        }
    }

    public int Order { get; set; } = 1;

    public void OnInitialize()
    {
        AutoLoad();
        Initialize();
    }

    protected abstract void Initialize();

    protected abstract void Dispose(bool dispose);

    protected virtual void AutoLoad()
    {
        foreach (var type in GetType().Assembly.GetTypes())
        {
            if (type.IsDefined(typeof(ConfigSeries), false))
            {
                var method = type.BaseType!.GetMethod("Load") ?? throw new MissingMethodException($"method 'Load()' is missing inside the lazy loaded config class '{Name}'");
                var name = method.Invoke(null, Array.Empty<object>());
                Logger.LogInformation($"[{Name}] config registered: {name}");
            }

        }
        XocMatAPI.CommandManager.RegisterCommand(GetType().Assembly).ForEach(c=> Logger.LogInformation($"Command {c.Alias.First()} Register Successfully!"));
    }

    public void Dispose()
    {
        foreach (var type in GetType().Assembly.GetTypes())
        {
            if (type.IsDefined(typeof(ConfigSeries), false))
            {
                var method = type.BaseType!.GetMethod("UnLoad") ?? throw new MissingMethodException($"method 'UnLoad()' is missing inside the lazy loaded config class '{Name}'");
                var name = method.Invoke(null, []);
                Logger.LogInformation($"[{Name}] Config UnRegistered: {name}");
            }
        }
        XocMatAPI.CommandManager.Commands.RemoveAll(c => c.GetType().Assembly == GetType().Assembly);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~XocMatPlugin()
    {
        Dispose(true);
    }
}

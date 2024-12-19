using Lagrange.Core;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Commands;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public abstract class XocMatPlugin : IDisposable
{
    protected XocMatPlugin(ILogger logger, CommandManager commandManager, BotContext bot)
    {
        Logger = logger;
        CommandManager = commandManager;
        BotContext = bot;
        AutoLoad();
    }
    public ILogger Logger { get; }

    public CommandManager CommandManager { get; }

    public BotContext BotContext { get; }


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

    public abstract void Initialize();

    protected abstract void Dispose(bool dispose);

    internal void AutoLoad()
    {
        foreach (var type in GetType().Assembly.GetTypes())
        {
            if (type.IsDefined(typeof(ConfigSeries), false))
            {
                var method = type.BaseType!.GetMethod("Load") ?? throw new MissingMethodException($"method 'Load()' is missing inside the lazy loaded config class '{Name}'");
                var name = method.Invoke(null, []);
                Logger.LogInformation($"[{Name}] config registered: {name}");
            }
            else if (type.IsDefined(typeof(CommandSeries), false))
            {
                CommandManager.RegisterCommand(type);
            }
            
        }
    }

    ~XocMatPlugin()
    {
        Dispose(true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


}

using Lagrange.Core;
using Lagrange.XocMat.Command;
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
        CommandManager.RegisterCommand(GetType().Assembly);
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

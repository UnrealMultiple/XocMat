using Lagrange.Core;
using Lagrange.XocMat.Commands;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public abstract class XocMatPlugin(ILogger logger, CommandManager commandManager, BotContext bot) : IDisposable
{
    public ILogger Logger { get; } = logger;

    public CommandManager CommandManager { get; } = commandManager;

    public BotContext Bot { get; } = bot;

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

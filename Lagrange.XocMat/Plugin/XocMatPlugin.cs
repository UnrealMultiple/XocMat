using Lagrange.Core;
using Lagrange.XocMat.Attributes;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Plugin;

public abstract class XocMatPlugin : IDisposable
{
    protected List<Command.Command> AssemblyCommands = [];

    protected XocMatPlugin(ILogger logger, BotContext bot)
    {
        Logger = logger;
        BotContext = bot;
        foreach (var type in GetType().Assembly.GetTypes())
        {
            if (type.IsDefined(typeof(ConfigSeries), false))
            {
                var method = type.BaseType!.GetMethod("Load") ?? throw new MissingMethodException($"method 'Load()' is missing inside the lazy loaded config class '{Name}'");
                var name = method.Invoke(null, Array.Empty<object>());
                Logger.LogInformation($"[{Name}] config registered: {name}");
            }
           
        }
    }
    public ILogger Logger { get; }

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

    protected virtual void DisableCommand()
    { 
        foreach(var command in AssemblyCommands)
        {
            XocMatAPI.CommandManager.Commands.Remove(command);
        }
    }

    public int Order { get; set; } = 1;

    public virtual void Initialize()
    { 
        AutoLoad();
    }

    protected virtual void Dispose(bool dispose)
    { 
        if(dispose)
            DisableCommand();
    }

    protected virtual void AutoLoad()
    {
        AssemblyCommands = XocMatAPI.CommandManager.RegisterCommand(GetType().Assembly);
        foreach(var command in AssemblyCommands)
        {
            Logger.LogInformation($"Command {command.Alias.First()} Register Successfully!");
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

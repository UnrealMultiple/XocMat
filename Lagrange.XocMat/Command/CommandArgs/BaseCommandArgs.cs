using Lagrange.Core;

namespace Lagrange.XocMat.Command.CommandArgs;

public class BaseCommandArgs(BotContext bot, string name, string prefix, List<string> param, Dictionary<string, string> cmdLine) : System.EventArgs
{
    public BotContext Bot { get; set; } = bot;
    public string Name { get; } = name;

    public string CommamdPrefix { get; } = prefix;

    public List<string> Parameters { get; } = param;

    public Dictionary<string, string> CommamdLine { get; } = cmdLine;

    public bool Handler { get; set; }
}

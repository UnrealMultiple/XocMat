using System.Drawing;
using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;

namespace Lagrange.XocMat.Command.CommandArgs;

public class BaseCommandArgs(BotContext bot, string name, string prefix, List<string> param, Dictionary<string, string> cmdLine) : System.EventArgs
{
    public BotContext Bot { get; set; } = bot;
    public string Name { get; } = name;

    public string CommandPrefix { get; } = prefix;

    public List<string> Parameters { get; } = param;

    public Dictionary<string, string> CommamdLine { get; } = cmdLine;

    public bool Handler { get; set; }

    public virtual string ToPreviewString() => $"[Command({CommandPrefix}{Name})] [Parameters]: {Parameters.JoinToString(",")} ";

    public virtual string ToPerviewErrorString(Exception e) => $"[CommandError({CommandPrefix}{Name})] [ErrorText]: {e} ";

    public virtual string ToSkippingString() => string.Empty;

}

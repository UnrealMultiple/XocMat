using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using System.Drawing;

namespace Lagrange.XocMat.Command;

public abstract class Command
{
    public virtual string[] Name { get; } = [];

    public virtual string HelpText { get; } = string.Empty;

    public virtual string Permission { get; } = string.Empty;

    public virtual async Task InvokeAsync(GroupCommandArgs args)
    {
        await args.EventArgs.Reply("This command is not available in this context.");
    }

    public virtual async Task InvokeAsync(ServerCommandArgs args)
    {
        if (args.Server != null)
            await args.Server.PrivateMsg(args.UserName, "This command is not available in this context.", Color.GreenYellow);
    }
}

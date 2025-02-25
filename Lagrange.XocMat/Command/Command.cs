using System.Drawing;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command;

public abstract class Command
{
    public virtual string[] Alias { get; protected set; } = [];

    public virtual string HelpText { get; } = string.Empty;

    public virtual string[] Permissions { get; protected set; } = [];

    public virtual async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        await args.Event.Reply("This command is not available in this context.");
    }

    public virtual async Task InvokeAsync(FriendCommandArgs args, ILogger log)
    {
        await args.Event.Reply("This command is not available in this context.");
    }

    public virtual async Task InvokeAsync(ServerCommandArgs args, ILogger log)
    {
        if (args.Server != null)
            await args.Server.PrivateMsg(args.UserName, "This command is not available in this context.", Color.GreenYellow);
    }
}

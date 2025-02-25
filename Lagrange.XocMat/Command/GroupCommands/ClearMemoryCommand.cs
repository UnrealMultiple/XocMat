using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ClearMemoryCommand : Command
{
    public override string[] Alias => ["清理内存"];

    public override string HelpText => "清理机子内存空间";

    public override string[] Permissions => [OneBotPermissions.ClearMemory];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        ulong old = SystemHelper.GetUsedPhys();
        SystemHelper.FreeMemory();
        ulong curr = old - SystemHelper.GetUsedPhys();
        await args.Event.Reply($"已释放内存:{SystemHelper.FormatSize(curr)}");
    }
}

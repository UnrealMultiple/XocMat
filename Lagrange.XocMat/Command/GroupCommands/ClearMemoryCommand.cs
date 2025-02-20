using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command.InternalCommands;

public class ClearMemoryCommand : Command
{
    public override string[] Name => ["清理内存"];

    public override string HelpText => "清理机子内存空间";

    public override string Permission => OneBotPermissions.ClearMemory;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        var old = SystemHelper.GetUsedPhys();
        SystemHelper.FreeMemory();
        var curr = old - SystemHelper.GetUsedPhys();
        await args.EventArgs.Reply($"已释放内存:{SystemHelper.FormatSize(curr)}");
    }
}

using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command.InternalCommands;

public class SearchItemCommand : Command
{
    public override string[] Name => ["sitem"];

    public override string HelpText => "在Terraria中搜索物品ID";

    public override string Permission => OneBotPermissions.SearchItem;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count > 0)
        {
            var items = SystemHelper.GetItemByIdOrName(args.Parameters[0]);
            await args.EventArgs.Reply(items.Count == 0 ? "未查询到指定物品" : $"查询结果:\n{string.Join(",", items.Select(x => $"{x.Name}({x.netID})"))}");
        }
    }
}

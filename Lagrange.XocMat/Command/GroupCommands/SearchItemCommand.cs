using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Internet;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SearchItemCommand : Command
{
    public override string[] Alias => ["sitem"];

    public override string HelpText => "在Terraria中搜索物品ID";

    public override string[] Permissions => [OneBotPermissions.SearchItem];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count > 0)
        {
            List<Item> items = TerrariaHelper.GetItemByIdOrName(args.Parameters[0]);
            await args.Event.Reply(items.Count == 0 ? "未查询到指定物品" : $"查询结果:\n{string.Join(",", items.Select(x => $"{x.Name}({x.netID})"))}");
        }
    }
}

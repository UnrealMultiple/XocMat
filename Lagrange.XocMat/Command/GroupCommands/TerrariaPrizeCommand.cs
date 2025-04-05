using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaPrizeCommand : Command
{
    public override string[] Alias => ["泰拉奖池"];

    public override string HelpText => "查看泰拉奖池";

    public override string[] Permissions => [OneBotPermissions.TerrariaPrize];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var tableBuilder = TableBuilder.Create()
            .SetHeader("奖品ID", "奖品名称", "最大数量", "最小数量", "中奖概率")
            .SetTitle("泰拉奖池")
            .SetMemberUin(args.MemberUin);
        int id = 1;
        foreach (Internal.Terraria.Prize item in TerrariaPrize.Instance.Pool)
        {
            tableBuilder.AddRow(id.ToString(), item.Name, item.Max.ToString(), item.Min.ToString(), item.Probability.ToString());
            id++;
        }
        await args.MessageBuilder.Image(tableBuilder.Builder()).Reply();
    }
}
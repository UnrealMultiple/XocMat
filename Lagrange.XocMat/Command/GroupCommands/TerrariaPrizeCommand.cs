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
        TableBuilder tableBuilder = new TableBuilder()
            .SetTitle("泰拉奖池")
            .AddRow("奖品ID", "奖品名称", "最大数量", "最小数量", "中奖概率");
        int id = 1;
        foreach (Internal.Terraria.Prize item in TerrariaPrize.Instance.Pool)
        {
            tableBuilder.AddRow(id.ToString(), item.Name, item.Max.ToString(), item.Min.ToString(), item.Probability.ToString());
            id++;
        }
        Core.Message.MessageResult s = await args.MessageBuilder.Image(await tableBuilder.BuildAsync()).Reply();
        Console.WriteLine(s.Result);
    }
}
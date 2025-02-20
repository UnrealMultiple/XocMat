using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility.Images;

namespace Lagrange.XocMat.Command.InternalCommands;

public class TerrariaPrizeCommand : Command
{
    public override string[] Name => ["泰拉奖池"];

    public override string HelpText => "查看泰拉奖池";

    public override string Permission => OneBotPermissions.TerrariaPrize;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        var tableBuilder = new TableBuilder()
            .SetTitle("泰拉奖池")
            .AddRow("奖品ID", "奖品名称", "最大数量", "最小数量", "中奖概率");
        var id = 1;
        foreach (var item in TerrariaPrize.Instance.Pool)
        {
            tableBuilder.AddRow(id.ToString(), item.Name, item.Max.ToString(), item.Min.ToString(), item.Probability.ToString());
            id++;
        }
        var s = await args.MessageBuilder.Image(await tableBuilder.BuildAsync()).Reply();
        Console.WriteLine(s.Result);
    }
}
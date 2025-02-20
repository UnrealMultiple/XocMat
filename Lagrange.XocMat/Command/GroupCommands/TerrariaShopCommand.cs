using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility.Images;


namespace Lagrange.XocMat.Command.InternalCommands;

public class TerrariaShopCommand : Command
{
    public override string[] Name => ["泰拉商店"];

    public override string HelpText => "查看泰拉商店的商品";

    public override string Permission => OneBotPermissions.TerrariaShop;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        var tableBuilder = new TableBuilder()
            .SetTitle("泰拉商店")
            .AddRow("商品ID", "商品名称", "数量", "价格");
        var id = 1;
        foreach (var item in TerrariaShop.Instance.TrShop)
        {
            tableBuilder.AddRow(item.ID.ToString(), item.Name, item.Num.ToString(), item.Price.ToString());
            id++;
        }
        await args.MessageBuilder.Image(await tableBuilder.BuildAsync()).Reply();
    }
}

using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;


namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaShopCommand : Command
{
    public override string[] Alias => ["泰拉商店"];

    public override string HelpText => "查看泰拉商店的商品";

    public override string[] Permissions => [OneBotPermissions.TerrariaShop];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var tableBuilder = TableBuilder.Create()
            .SetHeader("序号", "商品ID", "商品名称", "数量", "价格")
            .SetTitle("泰拉商店")
            .SetMemberUin(args.MemberUin);
        int id = 1;
        foreach (Internal.Terraria.Shop item in TerrariaShop.Instance.TrShop)
        {
            tableBuilder.AddRow(id.ToString(), item.ID.ToString(), item.Name, item.Num.ToString(), item.Price.ToString());
            id++;
        }
        await args.MessageBuilder.Image(tableBuilder.Builder()).Reply();
    }
}

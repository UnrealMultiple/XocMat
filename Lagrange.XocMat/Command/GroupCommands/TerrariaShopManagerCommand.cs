using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Internal.Terraria;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaShopManagerCommand : Command
{
    public override string[] Alias => ["shop"];

    public override string HelpText => "管理商店";

    public override string[] Permissions => [OneBotPermissions.TerrariaShopAdmin];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 4 && args.Parameters[0].ToLower() == "add")
        {
            if (!int.TryParse(args.Parameters[1], out int id) || id < 1)
            {
                await args.Event.Reply("请输入一个正确的物品ID", true);
                return;
            }
            if (!int.TryParse(args.Parameters[2], out int cost) || cost < 0)
            {
                await args.Event.Reply("请输入一个正确价格", true);
                return;
            }
            if (!int.TryParse(args.Parameters[3], out int num) || num < 0)
            {
                await args.Event.Reply("请输入一个正确数量", true);
                return;
            }
            Internal.Socket.Internet.Item? item = SystemHelper.GetItemById(id);
            if (item != null)
            {
                TerrariaShop.Instance.TrShop.Add(new Shop(item.Name, id, cost, num));
                await args.Event.Reply("添加成功", true);
                TerrariaShop.Save();
            }
            else
            {
                await args.Event.Reply("没有找到此物品", true);
            }
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "del")
        {
            if (!int.TryParse(args.Parameters[1], out int index))
            {
                await args.Event.Reply("请输入一个正确序号", true);
                return;
            }
            Shop? shop = TerrariaShop.Instance.GetShop(index);
            if (shop == null)
            {
                await args.Event.Reply("该商品不存在!", true);
                return;
            }
            TerrariaShop.Instance.TrShop.Remove(shop);
            await args.Event.Reply("删除成功", true);
            TerrariaShop.Save();
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n" +
                $"{args.CommamdPrefix}{args.Name} add [物品id] [价格] [数量]\n" +
                $"{args.CommamdPrefix}{args.Name} del [商品序号]");
        }
    }
}

using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Permission;
using System.Drawing;
using System.Text;

namespace Lagrange.XocMat.Commands;

[CommandSeries]
public class ServerCommands
{
    [CommandMap("泰拉商店")]
    [CommandPermission(OneBotPermissions.TerrariaShop)]
    public static async ValueTask ShopList(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        var sb = new StringBuilder();
        var shop = TerrariaShop.Instance.TrShop;
        var index = 1;
        foreach (var item in shop)
        {
            sb.AppendLine($"{index}.{item.Name} x {item.Num}     {item.Price}");
            index++;
        }
        await args.Server.PrivateMsg(args.UserName, $"泰拉商店列表:\n{sb}", Color.GreenYellow);
    }

    [CommandMap("抽")]
    [CommandPermission(OneBotPermissions.TerrariaPrize)]
    public static async ValueTask Prize(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.User == null)
        {
            await args.Server.PrivateMsg(args.UserName, "没有你的注册信息！", Color.DarkRed);
            return;
        }
        if (!args.Server.EnabledPrize)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启抽奖系统！", Color.DarkRed);
            return;
        }
        var count = 1;
        if (args.Parameters.Count > 0)
            _ = int.TryParse(args.Parameters[0], out count);
        if (count > 50)
            count = 50;
        var prizes = TerrariaPrize.Instance.Nexts(count);
        if (prizes.Count == 0)
        {
            await args.Server.PrivateMsg(args.UserName, "奖池中空空如也哦!", Color.GreenYellow);
            return;
        }
        var curr = Currency.Query(args.User.GroupID, args.User.Id);
        if (curr == null || curr.Num < count * TerrariaPrize.Instance.Fess)
        {
            await args.Server.PrivateMsg(args.UserName, $"你的星币不足抽取{count}次", Color.Red);
            return;
        }
        Currency.Del(args.User.GroupID, args.User.Id, count * TerrariaPrize.Instance.Fess);
        Random random = new();
        foreach (var prize in prizes)
        {
            await args.Server.Command($"/g {prize.ID} {args.UserName} {random.Next(prize.Min, prize.Max)}");
        }
    }


    [CommandMap("购买")]
    [CommandPermission(OneBotPermissions.TerrariaShop)]
    public static async ValueTask ShopBuy(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.Parameters.Count != 1)
        {
            await args.Server.PrivateMsg(args.UserName, $"语法错误:\n正确语法:/购买 [名称|ID]", Color.GreenYellow);
            return;
        }
        if (!args.Server.EnabledShop)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启商店系统！", Color.DarkRed);
            return;
        }
        if (args.User != null)
        {
            if (int.TryParse(args.Parameters[0], out var id))
            {
                if (TerrariaShop.Instance.TryGetShop(id, out var shop) && shop != null)
                {
                    var curr = Currency.Query(args.User.GroupID, args.User.Id);
                    if (curr != null && curr.Num >= shop.Price)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.Num}");
                        if (res.Status)
                        {
                            Currency.Del(args.User.GroupID, args.User.Id, shop.Price);
                            await args.Server.PrivateMsg(args.UserName, "购买成功!", Color.GreenYellow);
                        }
                        else
                        {
                            await args.Server.PrivateMsg(args.UserName, "失败! 错误信息:\n" + res.Message, Color.GreenYellow);
                        }
                    }
                    else
                    {
                        await args.Server.PrivateMsg(args.UserName, "星币不足!", Color.GreenYellow);
                    }
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "该商品不存在!", Color.GreenYellow);
                }
            }
            else
            {
                if (TerrariaShop.Instance.TryGetShop(args.Parameters[0], out var shop) && shop != null)
                {
                    var curr = Currency.Query(args.User.GroupID, args.User.Id);
                    if (curr != null && curr.Num >= shop.Price)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.Num}");
                        if (res.Status)
                        {
                            Currency.Del(args.User.GroupID, args.User.Id, shop.Price);
                            await args.Server.PrivateMsg(args.UserName, "购买成功!", Color.GreenYellow);
                        }
                        else
                        {
                            await args.Server.PrivateMsg(args.UserName, "失败! 错误信息:\n" + res.Message, Color.GreenYellow);
                        }
                    }
                    else
                    {
                        await args.Server.PrivateMsg(args.UserName, "星币不足!", Color.GreenYellow);
                    }
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "该商品不存在!", Color.GreenYellow);
                }
            }
        }
        else
        {
            await args.Server.PrivateMsg(args.UserName, "未找到你的注册信息!", Color.GreenYellow);
        }
    }
}

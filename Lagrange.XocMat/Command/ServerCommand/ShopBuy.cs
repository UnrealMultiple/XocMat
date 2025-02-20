using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Permission;
using System.Drawing;

namespace Lagrange.XocMat.Command.ServerCommand;

public class ShopBuy : Command
{
    public override string[] Name => ["购买"];

    public override string HelpText => "购买商店物品";

    public override string Permission => OneBotPermissions.TerrariaShop;

    public override async Task InvokeAsync(ServerCommandArgs args)
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

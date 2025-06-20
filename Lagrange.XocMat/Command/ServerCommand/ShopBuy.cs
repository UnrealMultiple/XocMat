using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace Lagrange.XocMat.Command.ServerCommand;

public class ShopBuy : Command
{
    public override string[] Alias => ["购买"];

    public override string HelpText => "购买商店物品";

    public override string[] Permissions => [OneBotPermissions.TerrariaShop];

    public override async Task InvokeAsync(ServerCommandArgs args, ILogger log)
    {
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

        if (int.TryParse(args.Parameters[0], out int id))
        {
            if (TerrariaShop.Instance.TryGetShop(id, out Internal.Terraria.Shop? shop) && shop != null)
            {
                Currency? curr = Currency.Query(args.User.Id);
                if (curr != null && curr.Num >= shop.Price)
                {
                    Terraria.Protocol.Action.Response.ServerCommand res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.Num}");
                    if (res.Status)
                    {
                        Currency.Del(args.User.Id, shop.Price);
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
            if (TerrariaShop.Instance.TryGetShop(args.Parameters[0], out Internal.Terraria.Shop? shop) && shop != null)
            {
                Currency? curr = Currency.Query(args.User.Id);
                if (curr != null && curr.Num >= shop.Price)
                {
                    Terraria.Protocol.Action.Response.ServerCommand res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.Num}");
                    if (res.Status)
                    {
                        Currency.Del(args.User.Id, shop.Price);
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
}

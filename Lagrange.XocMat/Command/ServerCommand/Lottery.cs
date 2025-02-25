using System.Drawing;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.ServerCommand;

public class Lottery : Command
{
    public override string[] Alias => ["抽"];

    public override string HelpText => "抽取";

    public override string[] Permissions => [OneBotPermissions.TerrariaPrize];

    public override async Task InvokeAsync(ServerCommandArgs args, ILogger log)
    {
        if (!args.Server.EnabledPrize)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启抽奖系统！", Color.DarkRed);
            return;
        }
        int count = 1;
        if (args.Parameters.Count > 0)
            _ = int.TryParse(args.Parameters[0], out count);
        if (count > 50)
            count = 50;
        List<Internal.Terraria.Prize> prizes = TerrariaPrize.Instance.Nexts(count);
        if (prizes.Count == 0)
        {
            await args.Server.PrivateMsg(args.UserName, "奖池中空空如也哦!", Color.GreenYellow);
            return;
        }
        Currency? curr = Currency.Query(args.User.Id);
        if (curr == null || curr.Num < count * TerrariaPrize.Instance.Fess)
        {
            await args.Server.PrivateMsg(args.UserName, $"你的星币不足抽取{count}次", Color.Red);
            return;
        }
        Currency.Del(args.User.Id, count * TerrariaPrize.Instance.Fess);
        Random random = new();
        foreach (Internal.Terraria.Prize prize in prizes)
        {
            await args.Server.Command($"/g {prize.ID} {args.UserName} {random.Next(prize.Min, prize.Max)}");
        }
    }
}

using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class KillRank : Command
{
    public override string[] Alias => ["击杀排行"];
    public override string HelpText => "击杀排行";
    public override string[] Permissions => [OneBotPermissions.KillRank];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.PlayerStrikeBoss data = await server.GetStrikeBoss();
            if (data.Damages != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Internal.Socket.Internet.KillNpc? damage in data.Damages.OrderByDescending(x => x.KillTime))
                {
                    sb.AppendLine($"Boss: {damage.Name}");
                    sb.AppendLine($"总血量: {damage.MaxLife}");
                    sb.AppendLine($"更新时间: {damage.KillTime:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"状态: {(damage.IsAlive ? "未被击杀" : "已被击杀")}");
                    if (!damage.IsAlive)
                    {
                        sb.AppendLine($"击杀用时: {(damage.KillTime - damage.SpawnTime).TotalSeconds}秒");
                        sb.AppendLine($"丢失伤害: {damage.MaxLife - damage.Strikes.Sum(x => x.Damage)}");
                    }
                    foreach (Internal.Socket.Internet.PlayerStrike? strike in damage.Strikes.OrderByDescending(x => x.Damage))
                    {
                        sb.AppendLine($"{strike.Player}伤害 {Convert.ToSingle(strike.Damage) / damage.MaxLife * 100:F0}%({strike.Damage})");
                    }
                    sb.AppendLine();
                }
                await args.Event.Reply(sb.ToString().Trim());
            }
            else
            {
                await args.Event.Reply("暂无击杀数据可以统计!", true);
            }
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

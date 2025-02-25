using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class DeathRank : Command
{
    public override string[] Alias => ["死亡排行"];
    public override string HelpText => "死亡排行";
    public override string[] Permissions => [OneBotPermissions.DeathRank];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.DeadRank api = await server.DeadRank();
            Core.Message.MessageBuilder body = args.MessageBuilder;
            if (api.Status)
            {
                if (api.Rank.Count == 0)
                {
                    await args.Event.Reply("当前还没有数据记录", true);
                    return;
                }
                StringBuilder sb = new StringBuilder($"[{server.Name}]死亡排行:\n");
                IOrderedEnumerable<KeyValuePair<string, int>> rank = api.Rank.OrderByDescending(x => x.Value);
                foreach ((string name, int count) in rank)
                {
                    sb.AppendLine($"[{name}]死亡次数: {count}");
                }
                body.Text(sb.ToString().Trim());
            }
            else
            {
                body.Text("无法连接到服务器！");
            }
            await args.Event.Reply(body);
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

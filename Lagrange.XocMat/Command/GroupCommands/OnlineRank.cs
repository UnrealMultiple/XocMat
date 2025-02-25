using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class OnlineRank : Command
{
    public override string[] Alias => ["在线排行"];
    public override string HelpText => "在线排行";
    public override string[] Permissions => [OneBotPermissions.OnlineRank];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.PlayerOnlineRank api = await server.OnlineRank();
            Core.Message.MessageBuilder body = args.MessageBuilder;
            if (api.Status)
            {
                if (api.OnlineRank.Count == 0)
                {
                    await args.Event.Reply("当前还没有数据记录", true);
                    return;
                }
                StringBuilder sb = new StringBuilder($"[{server.Name}]在线排行:\n");
                IOrderedEnumerable<KeyValuePair<string, int>> rank = api.OnlineRank.OrderByDescending(x => x.Value);
                foreach ((string name, int duration) in rank)
                {
                    int day = duration / (60 * 60 * 24);
                    int hour = (duration - (day * 60 * 60 * 24)) / (60 * 60);
                    int minute = (duration - (day * 60 * 60 * 24) - (hour * 60 * 60)) / 60;
                    int second = duration - (day * 60 * 60 * 24) - (hour * 60 * 60) - (minute * 60);
                    sb.Append($"[{name}]在线时长: ");
                    if (day > 0)
                        sb.Append($"{day}天");
                    if (hour > 0)
                        sb.Append($"{hour}时");
                    if (minute > 0)
                        sb.Append($"{minute}分");
                    sb.Append($"{second}秒\n");
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

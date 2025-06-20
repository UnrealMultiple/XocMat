using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;
using System.Text;

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
            PlayerOnlineRank api = await server.OnlineRank();
            if (api.Status)
            {
                if (api.OnlineRank.Count == 0)
                {
                    await args.Event.Reply("当前还没有数据记录", true);
                    return;
                }
                var builder = new ProfileItemBuilder()
                    .SetTitle($"[{server.Name}]在线排行")
                    .SetMemberUin(args.MemberUin);
                IOrderedEnumerable<KeyValuePair<string, int>> rank = api.OnlineRank.OrderByDescending(x => x.Value);
                foreach ((string name, int duration) in rank)
                {
                    var sb = new StringBuilder();
                    int day = duration / (60 * 60 * 24);
                    int hour = (duration - (day * 60 * 60 * 24)) / (60 * 60);
                    int minute = (duration - (day * 60 * 60 * 24) - (hour * 60 * 60)) / 60;
                    int second = duration - (day * 60 * 60 * 24) - (hour * 60 * 60) - (minute * 60);
                    if (day > 0)
                        sb.Append($"{day}天");
                    if (hour > 0)
                        sb.Append($"{hour}时");
                    if (minute > 0)
                        sb.Append($"{minute}分");
                    sb.Append($"{second}秒");
                    builder.AddItem(name, sb.ToString());
                }
                await args.MessageBuilder
                    .Image(builder.Build())
                    .Reply();


            }
            else
            {
                await args.Event.Reply("无法连接到服务器！", true);
            }
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

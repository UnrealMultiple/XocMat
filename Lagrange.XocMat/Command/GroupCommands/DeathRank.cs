using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Utility.Images;
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
            DeadRank api = await server.DeadRank();
            Core.Message.MessageBuilder body = args.MessageBuilder;
            if (api.Status)
            {
                if (api.Rank.Count == 0)
                {
                    await args.Event.Reply("当前还没有数据记录", true);
                    return;
                }
                var builder = new ProfileItemBuilder()
                    .SetMemberUin(args.MemberUin)
                    .SetTitle("死亡排行");
                IOrderedEnumerable<KeyValuePair<string, int>> rank = api.Rank.OrderByDescending(x => x.Value);
                foreach ((string name, int count) in rank)
                {
                    builder.AddItem(name, count.ToString());
                }
                await args.MessageBuilder
                    .Image(builder.Build())
                    .Reply();
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

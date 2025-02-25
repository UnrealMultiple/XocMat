using Lagrange.Core.Message;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ReStartServer : Command
{
    public override string[] Alias => ["重启服务器"];
    public override string HelpText => "重启服务器";
    public override string[] Permissions => [OneBotPermissions.ResetTShock];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.BaseActionResponse api = await server.ReStartServer(args.CommamdLine);
            MessageBuilder build = MessageBuilder.Group(args.GroupUin);
            if (api.Status)
            {
                build.Text("正在重启服务器，请稍后...");
            }
            else
            {
                build.Text(api.Message);
            }
            await args.Event.Reply(build);
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

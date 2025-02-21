using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Picture;

namespace Lagrange.XocMat.Command.GroupCommands;

public class GameProgress : Command
{
    public override string[] Alias => ["进度查询"];
    public override string HelpText => "进度查询";
    public override string[] Permissions => [OneBotPermissions.QueryProgress];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            Internal.Socket.Action.Response.GameProgress api = await server.QueryServerProgress();
            Core.Message.MessageBuilder body = args.MessageBuilder;
            if (api.Status)
            {
                MemoryStream stream = ProgressImage.Start(api.Progress, server.Name);
                body.Image(stream.ToArray());
            }
            else
            {
                body.Text("无法获取服务器信息！");
            }
            await args.Event.Reply(body);
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

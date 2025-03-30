using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Terraria.Protocol.Internet;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ServerInfo : Command
{
    public override string[] Alias => ["插件列表"];
    public override string HelpText => "服务器信息";
    public override string[] Permissions => [OneBotPermissions.ServerList];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            ServerStatus status = await server.ServerStatus();
            if (!status.Status)
            {
                await args.Event.Reply("无法连接服务器!", true);
                return;
            }
            var tableBuilder = TableBuilder.Create()
                .SetHeader("插件名称", "插件说明", "插件作者")
                .SetTitle("插件列表")
                .SetMemberUin(args.MemberUin);
            foreach (PluginInfo plugin in status.Plugins)
            {
                tableBuilder.AddRow(plugin.Name, plugin.Description, plugin.Author);
            }
            await args.MessageBuilder.Image(tableBuilder.Builder()).Reply();
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

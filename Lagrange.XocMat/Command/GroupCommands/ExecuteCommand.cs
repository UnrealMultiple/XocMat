using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ExecuteCommand : Command
{
    public override string[] Alias => ["执行"];
    public override string HelpText => "执行命令";
    public override string[] Permissions => [OneBotPermissions.ExecuteCommand];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count < 1)
        {
            await args.Event.Reply("请输入要执行的命令!", true);
            return;
        }
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            string cmd = "/" + string.Join(" ", args.Parameters);
            Terraria.Protocol.Action.Response.ServerCommand api = await server.Command(cmd);
            Core.Message.MessageBuilder body = args.MessageBuilder;
            if (api.Status)
            {
                string cmdResult = $"[{server.Name}]命令执行结果:\n{string.Join("\n", api.Params)}";
                body.Text(cmdResult);
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

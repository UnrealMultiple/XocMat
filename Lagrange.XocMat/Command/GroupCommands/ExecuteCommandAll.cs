using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ExecuteCommandAll : Command
{
    public override string[] Alias => ["执行全部"];
    public override string HelpText => "执行全部命令";
    public override string[] Permissions => [OneBotPermissions.ExecuteCommand];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count < 1)
        {
            await args.Event.Reply("请输入要执行的命令!", true);
            return;
        }
        StringBuilder sb = new StringBuilder();
        foreach (Terraria.TerrariaServer server in XocMatSetting.Instance.Servers)
        {
            string cmd = "/" + string.Join(" ", args.Parameters);
            Terraria.Protocol.Action.Response.ServerCommand api = await server.Command(cmd);
            sb.AppendLine($"[{server.Name}]命令执行结果:");
            sb.AppendLine(api.Status ? string.Join("\n", api.Params) : "无法连接到服务器！");
        }
        await args.Event.Reply(sb.ToString().Trim());
    }
}

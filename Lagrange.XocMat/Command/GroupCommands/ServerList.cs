using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ServerList : Command
{
    public override string[] Alias => ["服务器列表"];
    public override string HelpText => "服务器列表";
    public override string[] Permissions => [OneBotPermissions.ServerList];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        IEnumerable<Terraria.TerrariaServer> groupServers = XocMatSetting.Instance.Servers.Where(s => s.Groups.Contains(args.GroupUin));
        if (!groupServers.Any())
        {
            await args.Event.Reply("此群未配置任何服务器!", true);
            return;
        }
        TableBuilder tableBuilder = new TableBuilder();
        tableBuilder.SetTitle("服务器列表");
        tableBuilder.AddRow("服务器名称", "服务器IP", "服务器端口", "服务器版本", "服务器介绍", "运行状态", "世界名称", "世界种子", "世界大小");
        foreach (Terraria.TerrariaServer? server in groupServers)
        {
            Internal.Socket.Action.Response.ServerStatus status = await server.ServerStatus();
            tableBuilder.AddRow(server.Name, server.IP, server.NatProt.ToString(), server.Version, server.Describe,
                !status.Status ? "无法连接" : $"已运行:{status.RunTime:dd\\.hh\\:mm\\:ss}",
                !status.Status ? "无法获取" : status.WorldName,
                !status.Status ? "无法获取" : status.WorldSeed,
                !status.Status ? "无法获取" : $"{status.WorldWidth}x{status.WorldHeight}");
        }

        await args.MessageBuilder.Image(await tableBuilder.BuildAsync()).Reply();
    }
}

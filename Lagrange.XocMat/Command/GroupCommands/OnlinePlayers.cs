using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System.Text;

namespace Lagrange.XocMat.Command.GroupCommands;

public class OnlinePlayers : Command
{
    public override string[] Alias => ["在线"];
    public override string HelpText => "查询在线玩家";
    public override string[] Permissions => [OneBotPermissions.QueryOnlienPlayer];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (XocMatSetting.Instance.Servers.Count == 0)
        {
            await args.Event.Reply("还没有配置任何一个服务器!", true);
            return;
        }
        var groupServers = XocMatSetting.Instance.Servers.Where(s => s.Groups.Contains(args.GroupUin));
        if (!groupServers.Any())
        {
            await args.Event.Reply("此群未配置任何服务器!", true);
            return;
        }
        //var builder = OnlineBuilder.Create();
        //foreach (var groupServer in groupServers)
        //{
        //    var api = await groupServer.ServerOnline();
        //    var rows = api.Players.Select(p => 
        //    {
        //        var user = TerrariaUser.GetUsersByName(p.Name, groupServer.Name);
        //        return user is null ? new OnlineCell(10086, p.Name, Color.Yellow) : new OnlineCell((uint)user.Id, p.Name, Color.DarkGreen);
        //    }).ToArray();
        //    var title = $"[{groupServer.Name}]{(api.Status ? $"({api.Players.Count}/{api.MaxCount})" : "无法连接服务器")}";
        //    builder.Add(title, rows);
        //}
        //await args.MessageBuilder.Image(builder.Build()).Reply();
        StringBuilder sb = new StringBuilder();
        foreach (Terraria.TerrariaServer? server in groupServers)
        {
            var api = await server.ServerOnline();

            sb.AppendLine($"[{server.Name}]在线玩家数量({(api.Status ? api.Players.Count : 0)}/{api.MaxCount})");
            sb.AppendLine(api.Status ? string.Join(",", api.Players.Select(x => x.Name)) : "无法连接服务器");
        }
        await args.Event.Reply(sb.ToString().Trim());
    }
}

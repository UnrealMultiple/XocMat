using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Picture;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class QueryInventory : Command
{
    public override string[] Alias => ["查背包"];
    public override string HelpText => "查询背包";
    public override string[] Permissions => [OneBotPermissions.QueryInventory];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 1)
        {
            if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
            {
                PlayerInventory api = await server.PlayerInventory(args.Parameters[0]);
                Core.Message.MessageBuilder body = args.MessageBuilder;
                if (api.Status)
                {
                    MemoryStream ms = DrawInventory.Start(api.PlayerData!, api.PlayerData!.Username, api.ServerName);
                    body.Image(ms.ToArray());
                }
                else
                {
                    body.Text("无法获取用户信息！");
                }
                await args.Event.Reply(body);
            }
            else
            {
                await args.Event.Reply("未切换服务器或服务器无效!", true);
            }
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommandPrefix}{args.Name} [用户名]");
        }
    }
}

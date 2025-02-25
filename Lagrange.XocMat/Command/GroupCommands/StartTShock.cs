using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class StartTShock : Command
{
    public override string[] Alias => ["启动"];
    public override string HelpText => "启动服务器";
    public override string[] Permissions => [OneBotPermissions.StartTShock];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            if (server.Start(args.CommamdLine))
            {
                await args.Event.Reply($"{server.Name} 正在对其执行启动命令!", true);
                return;
            }
            await args.Event.Reply($"{server.Name} 启动失败!", true);
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

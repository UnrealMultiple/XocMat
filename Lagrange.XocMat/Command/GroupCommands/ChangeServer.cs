using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ChangeServer : Command
{
    public override string[] Alias => ["切换"];
    public override string HelpText => "切换服务器";
    public override string[] Permissions => [OneBotPermissions.ChangeServer];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 1)
        {
            Terraria.TerrariaServer? server = XocMatSetting.Instance.GetServer(args.Parameters[0], args.GroupUin);
            if (server == null)
            {
                await args.Event.Reply("你切换的服务器不存在! 请检查服务器名称是否正确，此群是否配置服务器!");
                return;
            }
            UserLocation.Instance.Change(args.MemberUin, server);
            await args.Event.Reply($"已切换至`{server.Name}`服务器", true);
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}{args.Name} [服务器名称]");
        }
    }
}

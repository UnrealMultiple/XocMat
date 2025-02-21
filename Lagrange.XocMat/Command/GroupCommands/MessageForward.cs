using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class MessageForward : Command
{
    public override string[] Alias => ["消息转发"];
    public override string HelpText => "消息转发";
    public override string[] Permissions => [OneBotPermissions.ForwardMsg];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1)
        {
            if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
            {
                switch (args.Parameters[0])
                {
                    case "开启":
                    case "true":
                        server.ForwardGroups.Add(args.GroupUin);
                        await args.Event.Reply("开启成功", true);
                        break;
                    case "关闭":
                    case "false":
                        server.ForwardGroups.Remove(args.GroupUin);
                        await args.Event.Reply("关闭成功", true);
                        break;
                    default:
                        await args.Event.Reply("未知子命令！", true);
                        break;
                }
                XocMatSetting.Instance.SaveTo();
            }
            else
            {
                await args.Event.Reply("未切换服务器或服务器无效!", true);
            }
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}{args.Name} [开启|关闭]!", true);
        }
    }
}

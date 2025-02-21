using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class MuteAll : Command
{
    public override string[] Alias => ["全禁"];
    public override string HelpText => "全体禁言";
    public override string[] Permissions => [OneBotPermissions.Mute];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1)
        {
            switch (args.Parameters[0])
            {
                case "开启":
                case "开":
                case "true":
                    await args.Bot.MuteGroupGlobal(args.GroupUin, true);
                    await args.Event.Reply("开启成功！");
                    break;
                case "关闭":
                case "关":
                case "false":
                    await args.Bot.MuteGroupGlobal(args.GroupUin, false);
                    await args.Event.Reply("关闭成功");
                    break;
                default:
                    await args.Event.Reply("语法错误,正确语法:\n全禁 [开启|关闭]");
                    break;
            }
        }
        else
        {
            await args.Event.Reply("语法错误,正确语法:\n全禁 [开启|关闭]");
        }
    }
}

using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SetGroupName : Command
{
    public override string[] Alias => ["设置群名"];
    public override string HelpText => "设置群名";
    public override string[] Permissions => [OneBotPermissions.ChangeGroupOption];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1)
        {
            if (string.IsNullOrEmpty(args.Parameters[0]))
            {
                await args.Event.Reply("群名不能未空！");
                return;
            }
            await args.Bot.RenameGroup(args.GroupUin, args.Parameters[0]);
            await args.Event.Reply($"群名称已修改为`{args.Parameters[0]}`");
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}设置群名 [名称]");
        }
    }
}

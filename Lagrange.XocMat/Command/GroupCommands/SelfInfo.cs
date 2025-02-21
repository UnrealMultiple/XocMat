using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SelfInfo : Command
{
    public override string[] Alias => ["我的信息"];
    public override string HelpText => "查询自己的信息";
    public override string[] Permissions => [OneBotPermissions.SelfInfo];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        await args.Event.Reply(await CommandUtils.GetAccountInfo(args.GroupUin, args.MemberUin, args.Account.Group.Name));
    }
}

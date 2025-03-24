using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class AcountInfo : Command
{
    public override string[] Alias => ["查"];
    public override string HelpText => "查询他人信息";
    public override string[] Permissions => [OneBotPermissions.OtherInfo];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        IEnumerable<Core.Message.Entity.MentionEntity> at = args.Event.Chain.GetMention();
        if (at.Any())
        {
            Account group = Account.GetAccountNullDefault(at.First().Uin);
            await args.Event.Reply(CommandUtils.GetAccountInfo(args.GroupUin, at.First().Uin, group.Group.Name));
        }
        else if (args.Parameters.Count == 1 && uint.TryParse(args.Parameters[0], out uint id))
        {
            Account group = Account.GetAccountNullDefault(id);
            await args.Event.Reply(CommandUtils.GetAccountInfo(args.GroupUin, id, group.Group.Name));
        }
        else
        {
            await args.Event.Reply("查谁呢?", true);
        }
    }
}

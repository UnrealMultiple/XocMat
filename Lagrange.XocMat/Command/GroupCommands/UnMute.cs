using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class UnMute : Command
{
    public override string[] Alias => ["解"];
    public override string HelpText => "解除禁言";
    public override string[] Permissions => [OneBotPermissions.Mute];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 0)
        {
            IEnumerable<Core.Message.Entity.MentionEntity> atlist = args.Event.Chain.GetMention();
            if (!atlist.Any())
            {
                await args.Event.Reply("未指令目标成员!");
                return;
            }
            atlist.ForEach(async x => await args.Bot.MuteGroupMember(args.GroupUin, x.Uin, 0));
            await args.Event.Reply("解禁成功！");
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommandPrefix}解 [AT] [时长]！");
        }
    }
}

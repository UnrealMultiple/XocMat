using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class MuteMember : Command
{
    public override string[] Alias => ["禁"];
    public override string HelpText => "禁言群成员";
    public override string[] Permissions => [OneBotPermissions.Mute];
    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1)
        {
            if (!uint.TryParse(args.Parameters[0].ToString(), out uint muted))
            {
                await args.Event.Reply("请输入正确的禁言时长!");
                return;
            }
            IEnumerable<Core.Message.Entity.MentionEntity> atlist = args.Event.Chain.GetMention();
            if (!atlist.Any())
            {
                await args.Event.Reply("未指令目标成员!");
                return;
            }
            atlist.ForEach(async x => await args.Bot.MuteGroupMember(args.GroupUin, x.Uin, muted * 60));
            await args.Event.Reply("禁言成功！");
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}禁 [AT] [时长]！");
        }
    }
}
using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class UnsetGroupAdmin : Command
{
    public override string[] Alias => ["取消管理"];
    public override string HelpText => "取消管理";
    public override string[] Permissions => [OneBotPermissions.ChangeGroupOption];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            Core.Message.Entity.MentionEntity atlist = args.Event.Chain.GetMention().First();
            if (atlist != null)
            {
                await args.Bot.SetGroupAdmin(args.GroupUin, atlist.Uin, false);
                await args.Event.Reply($"已取消`{atlist.Uin}`的管理员!");
            }
            else
            {
                await args.Event.Reply("请选择一位成员！");
            }
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}{args.Name} [AT]");
        }
    }
}

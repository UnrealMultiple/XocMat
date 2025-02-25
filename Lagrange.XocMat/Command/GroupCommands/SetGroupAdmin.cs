using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SetGroupAdmin : Command
{
    public override string[] Alias => ["设置管理"];
    public override string HelpText => "设置管理";
    public override string[] Permissions => [OneBotPermissions.ChangeGroupOption];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 0)
        {
            Core.Message.Entity.MentionEntity atlist = args.Event.Chain.GetMention().First();
            if (atlist != null)
            {
                await args.Bot.SetGroupAdmin(args.GroupUin, atlist.Uin, true);
                await args.Event.Reply($"已将`{atlist.Uin}`设置为管理员!");
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

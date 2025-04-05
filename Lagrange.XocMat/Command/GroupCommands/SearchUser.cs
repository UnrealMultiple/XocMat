using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SearchUser : Command
{
    public override string[] Alias => ["注册查询", "name"];
    public override string HelpText => "查询注册人";
    public override string[] Permissions => [OneBotPermissions.SearchUser];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        async ValueTask GetRegister(long id)
        {
            List<TerrariaUser> users = TerrariaUser.GetUsers(id);
            if (users.Count == 0)
            {
                await args.Event.Reply("未查询到该用户的注册信息!", true);
                return;
            }
            var table = TableBuilder.Create()
                .SetHeader("注册名称", "注册账号", "群昵称")
                .SetTitle("注册查询")
                .SetMemberUin(args.MemberUin);
            foreach (TerrariaUser user in users)
            {
                Core.Common.Entity.BotGroupMember? result = (await args.Bot.FetchMembers(args.GroupUin)).FirstOrDefault(x => x.Uin == user.Id);
                if (result != null)
                {
                    table.AddRow(user.Name, user.Id.ToString(), result.MemberCard ?? result.MemberName);
                }
                else
                {
                    table.AddRow(user.Name, user.Id.ToString(), "注册人已消失!");
                }
            }
            await args.MessageBuilder.Image(table.Builder()).Reply();
        }
        IEnumerable<Core.Message.Entity.MentionEntity> atlist = args.Event.Chain.GetMention();
        if (args.Parameters.Count == 0 && atlist.Any())
        {
            Core.Message.Entity.MentionEntity target = atlist.First();
            await GetRegister(target.Uin);

        }
        else if (args.Parameters.Count == 1)
        {
            if (long.TryParse(args.Parameters[0], out long id))
            {
                await GetRegister(id);
            }
            else
            {
                TerrariaUser? user = TerrariaUser.GetUsersByName(args.Parameters[0]);
                if (user == null)
                {
                    await args.Event.Reply("未查询到注册信息", true);
                    return;
                }
                else
                {
                    await GetRegister(user.Id);
                }
            }
        }
    }
}

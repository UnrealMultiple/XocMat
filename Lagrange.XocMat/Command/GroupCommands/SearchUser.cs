using System.Text;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SearchUser : Command
{
    public override string[] Alias => ["注册查询", "name"];
    public override string HelpText => "查询注册人";
    public override string[] Permissions => [OneBotPermissions.SearchUser];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        async ValueTask GetRegister(long id)
        {
            List<TerrariaUser> users = TerrariaUser.GetUsers(id);
            if (users.Count == 0)
            {
                await args.Event.Reply("未查询到该用户的注册信息!");
                return;
            }
            StringBuilder sb = new("查询结果:\n");
            foreach (TerrariaUser user in users)
            {
                sb.AppendLine($"注册名称: {user.Name}");
                sb.AppendLine($"注册账号: {user.Id}");
                Core.Common.Entity.BotGroupMember? result = (await args.Bot.FetchMembers(args.GroupUin)).FirstOrDefault(x => x.Uin == user.Id);
                if (result != null)
                {
                    sb.AppendLine($"群昵称: {result.MemberName}");
                }
                else
                {
                    sb.AppendLine("注册人不在此群中");
                }
                sb.AppendLine("");
            }
            await args.Event.Reply(sb.ToString().Trim());
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

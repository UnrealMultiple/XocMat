using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using System.Text;

namespace Lagrange.XocMat.Command.InternalCommands;

public class TerrariaUserInfo : Command
{
    public override string[] Name => ["ui"];

    public override string HelpText => "查询tshock账户信息";
    
    public override string Permission => OneBotPermissions.QueryUserList;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (!UserLocation.Instance.TryGetServer(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.EventArgs.Chain.GroupUin!.Value, out var server) || server == null)
        {
            await args.EventArgs.Reply("服务器不存在或，未切换至一个服务器！", true);
            return;
        }
        if (args.Parameters.Count == 1)
        {
            var userName = args.Parameters[0];
            var info = await server.QueryAccount(userName);
            var account = info.Accounts.Find(x => x.Name == userName);
            if (!info.Status || account == null)
            {
                await args.EventArgs.Reply(info.Message, true);
                return;
            }

            var sb = new StringBuilder($"查询`{userName}\n");
            sb.AppendLine($"ID: {account.ID}");
            sb.AppendLine($"Group: {account.Group}");
            sb.AppendLine($"LastLogin: {account.LastLoginTime}");
            sb.AppendLine($"Registered: {account.RegisterTime}");
            await args.EventArgs.Reply(sb.ToString().Trim());
        }
        else
        {
            await args.EventArgs.Reply($"语法错误，正确语法:\n{args.CommamdPrefix}{args.Name} [名称]");
            return;
        }
    }
}

using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaUserInfo : Command
{
    public override string[] Alias => ["ui"];

    public override string HelpText => "查询tshock账户信息";

    public override string[] Permissions => [OneBotPermissions.QueryUserList];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (!UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) || server == null)
        {
            await args.Event.Reply("服务器不存在或，未切换至一个服务器！", true);
            return;
        }
        if (args.Parameters.Count == 1)
        {
            string userName = args.Parameters[0];
            Internal.Socket.Action.Response.QueryAccount info = await server.QueryAccount(userName);
            Internal.Socket.Internet.Account? account = info.Accounts.Find(x => x.Name == userName);
            if (!info.Status || account == null)
            {
                await args.Event.Reply(info.Message, true);
                return;
            }

            StringBuilder sb = new StringBuilder($"查询`{userName}\n");
            sb.AppendLine($"ID: {account.ID}");
            sb.AppendLine($"Group: {account.Group}");
            sb.AppendLine($"LastLogin: {account.LastLoginTime}");
            sb.AppendLine($"Registered: {account.RegisterTime}");
            await args.Event.Reply(sb.ToString().Trim());
        }
        else
        {
            await args.Event.Reply($"语法错误，正确语法:\n{args.CommamdPrefix}{args.Name} [名称]");
            return;
        }
    }
}

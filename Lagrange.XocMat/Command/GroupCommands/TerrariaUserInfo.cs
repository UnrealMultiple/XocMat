using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Terraria.Protocol.Internet;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class TerrariaUserInfo : Command
{
    public override string[] Alias => ["ui"];

    public override string HelpText => "查询tshock账户信息";

    public override string[] Permissions => [OneBotPermissions.QueryUserList];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (!UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) || server == null)
        {
            await args.Event.Reply("服务器不存在或，未切换至一个服务器！", true);
            return;
        }
        if (args.Parameters.Count == 1)
        {
            string userName = args.Parameters[0];
            QueryAccount info = await server.QueryAccount(userName);
            Account? account = info.Accounts.Find(x => x.Name == userName);
            if (!info.Status || account == null)
            {
                await args.Event.Reply(info.Message, true);
                return;
            }
            var builder = new ProfileItemBuilder()
                .SetMemberUin(args.MemberUin)
                .SetTitle($"[{server.Name}][{userName}]账户信息")
                .AddItem("ID", account.ID.ToString())
                .AddItem("Group", account.Group)
                .AddItem("LastLogin", account.LastLoginTime.ToString())
                .AddItem("Registered", account.RegisterTime.ToString());
            await args.MessageBuilder
                .Image(builder.Build())
                .Reply();
        }
        else
        {
            await args.Event.Reply($"语法错误，正确语法:\n{args.CommandPrefix}{args.Name} [名称]");
            return;
        }
    }
}

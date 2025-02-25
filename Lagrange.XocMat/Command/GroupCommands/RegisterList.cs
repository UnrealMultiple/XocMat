using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class RegisterList : Command
{
    public override string[] Alias => ["注册列表"];
    public override string HelpText => "注册列表";
    public override string[] Permissions => [OneBotPermissions.QueryUserList];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            List<TerrariaUser> users = TerrariaUser.GetUsers(server.Name);
            if (users == null || users.Count == 0)
            {
                await args.Event.Reply("注册列表空空如也!");
                return;
            }
            StringBuilder sb = new StringBuilder($"[{server.Name}]注册列表\n");
            foreach (TerrariaUser user in users)
            {
                sb.AppendLine($"{user.Name} => {user.Id}");
            }
            await args.Event.Reply(sb.ToString().Trim());
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

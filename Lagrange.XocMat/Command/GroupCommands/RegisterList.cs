using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
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
            var builder = new ProfileItemBuilder()
                .SetMemberUin(args.MemberUin)
                .SetTitle($"[{server.Name}]注册列表");
            foreach (TerrariaUser user in users)
            {
                builder.AddItem(user.Name, user.Id.ToString());
            }
            await args.MessageBuilder
                .Image(builder.Build())
                .Reply();
        }
        else
        {
            await args.Event.Reply("未切换服务器或服务器无效!", true);
        }
    }
}

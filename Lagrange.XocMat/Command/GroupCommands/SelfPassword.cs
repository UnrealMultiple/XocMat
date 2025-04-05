using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SelfPassword : Command
{
    public override string[] Alias => ["我的密码"];
    public override string HelpText => "查询自己的密码";
    public override string[] Permissions => [OneBotPermissions.SelfPassword];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            List<TerrariaUser> user = TerrariaUser.GetUserById(args.MemberUin, server.Name);
            if (user.Count > 0)
            {
                var body = user.Select(u => $"人物{u.Name}的注册密码为: {u.Password}").JoinToString("<br>");
                MailHelper.Builder(XocMatSetting.Instance.MailHost, XocMatSetting.Instance.SenderPwd)
                   .AddTarget($"{args.MemberUin}@qq.com")
                   .SetTile($"{server.Name}服务器注册密码")
                   .SetBody(CommandUtils.GenerateMailBody($"{server.Name}服务器注册密码查询", args.MemberUin, args.MemberCard, "请查看你的注册密码", body))
                   .EnableHtmlBody(true)
                   .SetSender(XocMatSetting.Instance.SenderMail)
                   .Send()
                   .Dispose();
                await args.Event.Reply("密码查询成功已发送至你的QQ邮箱。", true);
                return;
            }
            await args.Event.Reply($"{server.Name}上未找到你的注册信息。");
            return;
        }
        await args.Event.Reply("服务器无效或未切换至一个有效服务器!");
    }
}

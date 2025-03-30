using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class ResetPassword : Command
{
    public override string[] Alias => ["重置密码"];
    public override string HelpText => "重置密码";
    public override string[] Permissions => [OneBotPermissions.SelfPassword];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            try
            {
                List<TerrariaUser> user = TerrariaUser.GetUserById(args.MemberUin, server.Name);

                if (user.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (TerrariaUser u in user)
                    {
                        string pwd = Guid.NewGuid().ToString()[..8];
                        sb.Append($"人物 {u.Name}的密码重置为: {pwd}<br>");
                        BaseActionResponse res = await server.ResetPlayerPwd(u.Name, pwd);
                        if (!res.Status)
                        {
                            await args.Event.Reply("无法连接到服务器更改密码!");
                            return;
                        }
                        TerrariaUser.ResetPassword(args.MemberUin, server.Name, u.Name, pwd);
                    }
                    MailHelper.Builder(XocMatSetting.Instance.MailHost, XocMatSetting.Instance.SenderPwd)
                        .AddTarget($"{args.MemberUin}@qq.com")
                        .SetTile($"{server.Name}服密码重置")
                        .SetBody(CommandUtils.GenerateMailBody($"{server.Name}服务器绑定角色密码重置", args.MemberUin, args.MemberCard, "请查看重置密码", sb.ToString()))
                        .EnableHtmlBody(true)
                        .SetSender(XocMatSetting.Instance.SenderMail)
                        .Send()
                        .Dispose();
                    await args.Event.Reply("密码重置成功已发送至你的QQ邮箱。", true);
                    return;
                }
                await args.Event.Reply($"{server.Name}上未找到你的注册信息。");
                return;
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message, true);
            }
        }
        await args.Event.Reply("服务器无效或未切换至一个有效服务器!");
    }
}

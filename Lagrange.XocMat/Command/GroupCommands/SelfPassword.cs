using System.Text;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SelfPassword : Command
{
    public override string[] Alias => ["我的密码"];
    public override string HelpText => "查询自己的密码";
    public override string[] Permissions => [OneBotPermissions.SelfPassword];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) && server != null)
        {
            List<TerrariaUser> user = TerrariaUser.GetUserById(args.MemberUin, server.Name);
            if (user.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (TerrariaUser u in user)
                    sb.AppendLine($"人物{u.Name}的注册密码为: {u.Password}");
                sb.AppendLine("请注意保存不要暴露给他人");
                MailHelper.SendMail($"{args.MemberUin}@qq.com",
                            $"{server.Name}服务器注册密码",
                            sb.ToString().Trim());
                await args.Event.Reply("密码查询成功已发送至你的QQ邮箱。", true);
                return;
            }
            await args.Event.Reply($"{server.Name}上未找到你的注册信息。");
            return;
        }
        await args.Event.Reply("服务器无效或未切换至一个有效服务器!");
    }
}

using System.Text.RegularExpressions;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class Register : Command
{
    public override string[] Alias => ["注册"];
    public override string HelpText => "注册";
    public override string[] Permissions => [OneBotPermissions.RegisterUser];

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count == 1)
        {
            if (!UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out Terraria.TerrariaServer? server) || server == null)
            {
                await args.Event.Reply("未切换服务器或服务器无效!", true);
                return;
            }
            if (args.Parameters[0].Length > server.RegisterNameMax)
            {
                await args.Event.Reply($"注册的人物名称不能大于{server.RegisterNameMax}个字符!", true);
                return;
            }
            if (!new Regex("^[a-zA-Z0-9\u4e00-\u9fa5\\[\\]:/ ]+$").IsMatch(args.Parameters[0]) && server.RegisterNameLimit)
            {
                await args.Event.Reply("注册的人物名称不能包含中文,字母,数字和/:[]以外的字符", true);
                return;
            }
            if (TerrariaUser.GetUserById(args.MemberUin, server.Name).Count >= server.RegisterMaxCount)
            {
                await args.Event.Reply($"同一个服务器上注册账户不能超过{server.RegisterMaxCount}个", true);
                return;
            }
            string pass = Guid.NewGuid().ToString()[..8];
            try
            {
                TerrariaUser.Add(args.MemberUin, args.GroupUin, server.Name, args.Parameters[0], pass);
                Internal.Socket.Action.Response.BaseActionResponse api = await server.Register(args.Parameters[0], pass);
                Core.Message.MessageBuilder build = args.MessageBuilder;
                if (api.Status)
                {
                    MailHelper.SendMail($"{args.MemberUin}@qq.com",
                        $"{server.Name}服务器注册密码",
                        $"您的注册密码是:{pass}<br>请注意保存不要暴露给他人");
                    build.Text($"注册成功!" +
                        $"\n注册服务器: {server.Name}" +
                        $"\n注册名称: {args.Parameters[0]}" +
                        $"\n注册账号: {args.MemberUin}" +
                        $"\n注册人昵称: {args.Event.Chain.GroupMemberInfo!.MemberName}" +
                        $"\n注册密码已发送至QQ邮箱请点击下方链接查看" +
                        $"\nhttps://wap.mail.qq.com/home/index" +
                        $"\n进入服务器后可使用/password [当前密码] [新密码] 修改你的密码");
                }
                else
                {
                    TerrariaUser.Remove(server.Name, args.Parameters[0]);
                    build.Text(string.IsNullOrEmpty(api.Message) ? "无法连接服务器！" : api.Message);
                }
                await args.Event.Reply(build);
            }
            catch (TerrariaUserException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.Event.Reply($"语法错误,正确语法:\n{args.CommamdPrefix}{args.Name} [名称]");
        }
    }
}

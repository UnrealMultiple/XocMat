using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Microsoft.Extensions.Logging;

namespace PluginExample;

//继承Command类
public class CommandExample : Command
{
    public override string[] Alias => ["生成密码", "pwd"];

    public override string HelpText => "随机生成一个密码 ";

    public override string[] Permissions => ["onebot.password"];

    private static string GeneratePassWord()
    {
        return Guid.NewGuid().ToString()[..8];
    }

    //好友指令回复
    public override async Task InvokeAsync(FriendCommandArgs args, ILogger log)
    {
        
        
        await args.Event.Reply(GeneratePassWord());
    }

    //群聊指令回复
    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        var msg = args.Event.Chain.GetMsg<ForwardEntity>();
        Console.WriteLine(msg.Count());
        if (msg.FirstOrDefault() is ForwardEntity forwardEntity)
        {
            Console.WriteLine("进来");
            var message = MessageRecord.Query(forwardEntity.MessageId);
            if (message != null)
            {
                await args.Bot.SendMessage(message);
            }
        }
    }

    //服务器指令回复（TShock服务器指令）
    public override async Task InvokeAsync(ServerCommandArgs args, ILogger log)
    {
        await args.Reply(GeneratePassWord(), System.Drawing.Color.GreenYellow);
    }
}

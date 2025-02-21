using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Plugin;
using Microsoft.Extensions.Logging;

namespace PluginExample;

public class Plugin(ILogger logger, BotContext bot) : XocMatPlugin(logger, bot)
{
    //入口方法
    public override void Initialize()
    {
        //订阅群聊事件
        BotContext.Invoker.OnGroupMessageReceived += Invoker_OnGroupMessageReceived;

        //订阅指令事件
        Lagrange.XocMat.Event.OperatHandler.OnGroupCommand += OperatHandler_OnGroupCommand;
        //调用父类（调用父类是为了自动加载指令）
        base.Initialize();
    }

    //释放资源
    protected override void Dispose(bool dispose)
    {
        //取消订阅群聊事件
        BotContext.Invoker.OnGroupMessageReceived -= Invoker_OnGroupMessageReceived;
        //取消订阅指令事件
        Lagrange.XocMat.Event.OperatHandler.OnGroupCommand -= OperatHandler_OnGroupCommand;
        //调用父类（调用父类是为了自动卸载指令）
        base.Dispose(true);
    }

    private async ValueTask OperatHandler_OnGroupCommand(Lagrange.XocMat.Command.CommandArgs.GroupCommandArgs args)
    {
        if(ExampleConfig.Instance.DisabledCommands.Contains(args.Name))
        {
            await args.Event.Reply("该指令已经被拦截。", true);
            args.Handler = true;
        }
    }

    private void Invoker_OnGroupMessageReceived(BotContext context, Lagrange.Core.Event.EventArg.GroupMessageEvent e)
    {
        //群聊复读机功能
        if (e.Chain.GroupMemberInfo!.Uin != context.BotUin)
            context.SendMessage(e.Chain);
    }
}

using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Microsoft.Extensions.Hosting;


namespace Lagrange.XocMat;

public class XocMatAPI : BackgroundService
{
    public BotContext BotContext { get; private set; }

    public XocMatAPI(BotContext botContext)
    {
        BotContext = botContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        BotContext.Invoker.OnGroupMessageReceived += (BotContext bot, GroupMessageEvent e) =>
        {
            Console.WriteLine(e.Chain.GroupUin);
            BotContext.SendMessage(MessageBuilder.Group(e.Chain.GroupUin.GetValueOrDefault()).Text("回复").Build());
        };
        
    }
}

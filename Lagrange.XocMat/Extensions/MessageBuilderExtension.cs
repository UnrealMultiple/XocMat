using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.XocMat.Extensions;

public static class MessageBuilderExtension
{
    public static MessageBuilder MultiMsg(this MessageBuilder builder, MessageChain chains)
    {
        var m = new MultiMsgEntity([chains]);
        builder.Add(m);
        return builder;
    }

    public static MessageBuilder MultiMsg(this MessageBuilder builder, params MessageChain[] chains)
    {
        var m = new MultiMsgEntity([.. chains]);
        builder.Add(m);
        return builder;
    }

    public static async Task<MessageResult> Reply(this MessageBuilder message)
    {
        return await XocMatAPI.BotContext.Reply(message);
    }
}

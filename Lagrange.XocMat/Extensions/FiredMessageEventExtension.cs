using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.XocMat.Extensions;

public static class FiredMessageEventExtension
{
    public static async Task<MessageResult> Reply(this FriendMessageEvent e, MessageBuilder builder, bool type = false)
    {
        return await XocMatAPI.BotContext.Reply(builder);
    }

    public static async Task<MessageResult> Reply(this FriendMessageEvent e, string text, bool type = false)
    {
        MessageBuilder builder = MessageBuilder.Friend(e.Chain.FriendUin).Text(text);
        if (type)
        {
            var forwardEntity = new ForwardEntity(e.Chain);
            builder.Add(forwardEntity);

        }
        return await e.Reply(builder);
    }
}

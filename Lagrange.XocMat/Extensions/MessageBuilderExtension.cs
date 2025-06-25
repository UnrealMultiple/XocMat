using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.XocMat.Extensions;

public static class MessageBuilderExtension
{
    public static async Task<MessageResult> Reply(this MessageBuilder message)
    {
        return await XocMatAPI.BotContext.Reply(message);
    }
}

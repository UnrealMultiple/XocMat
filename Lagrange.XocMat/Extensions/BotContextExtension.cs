

using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.XocMat.Extensions;

public static class BotContextExtension
{
    public static async Task<MessageResult> Reply(this BotContext bot, MessageBuilder builder)
    {
        return await bot.SendMessage(builder.Build());
    }
}

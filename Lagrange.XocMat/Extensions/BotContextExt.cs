

using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;

namespace Lagrange.XocMat.Extensions;

public static class BotContextExt
{
    public static async Task Reply(this BotContext bot, MessageBuilder builder)
    {
        await bot.SendMessage(builder.Build());
    }
}

using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Extensions;

public static class MessageBuilderExtension
{
    public static MessageBuilder MarkdownImage(this MessageBuilder builder, string content)
    {
        try
        {
            byte[] buffer = MarkdownHelper.ToImage(content).Result;
            return builder.Image(buffer);
        }
        catch (Exception ex)
        {
            return builder.Text(ex.Message);
        }
    }

    public static MessageBuilder MultiMsg(this MessageBuilder builder, List<MessageChain> chains)
    {
        MultiMsgEntity m = new MultiMsgEntity(chains);
        builder.Add(m);
        return builder;
    }

    public static async Task<MessageResult> Reply(this MessageBuilder message)
    {
        return await XocMatAPI.BotContext.Reply(message);
    }
}

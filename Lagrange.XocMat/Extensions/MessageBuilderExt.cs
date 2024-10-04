using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.XocMat.Utility;

namespace Lagrange.XocMat.Extensions;

public static class MessageBuilderExt
{
    public static MessageBuilder MarkdownImage(this MessageBuilder builder, string content)
    {
        try
        {
            var buffer = MarkdownHelper.ToImage(content).Result;
            return builder.Image(buffer);
        }
        catch (Exception ex)
        {
            return builder.Text(ex.Message);
        }
    }
}

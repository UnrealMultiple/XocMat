using Lagrange.Core.Message;
using MessagePack;
using MessagePack.Formatters;

namespace Lagrange.XocMat.Entity;

public class MessageEntityResolver : IFormatterResolver
{
    private static readonly MessageEntityFormatter ENTITY_FORMATTER = new();

    private static readonly MessageChainFormatter CHAIN_FORMATTER = new();

    private static readonly StreamFormatter STREAM_FORMATTER = new();

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (typeof(T) == typeof(IMessageEntity)) return (IMessagePackFormatter<T>)ENTITY_FORMATTER;

        if (typeof(T) == typeof(MessageChain)) return (IMessagePackFormatter<T>)CHAIN_FORMATTER;

        if(typeof(T) == typeof(Stream)) return (IMessagePackFormatter<T>)STREAM_FORMATTER;

        return null;
    }
}

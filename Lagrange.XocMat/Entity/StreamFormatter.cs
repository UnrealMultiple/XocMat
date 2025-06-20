using MessagePack;
using MessagePack.Formatters;
using System.Buffers;

namespace Lagrange.XocMat.Entity;

public class StreamFormatter : IMessagePackFormatter<Stream?>
{
    public void Serialize(ref MessagePackWriter writer, Stream? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        // 将 Stream 转换为 byte[]
        byte[] buffer = new byte[value.Length];
        value.ReadExactly(buffer);
        writer.Write(buffer);
    }

    public Stream? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;

        // 从 byte[] 还原为 MemoryStream
        var sequence = reader.ReadBytes();
        if (sequence.HasValue)
        {
            byte[] buffer = sequence.Value.ToArray();
            return new MemoryStream(buffer);
        }
        return null;
    }
}

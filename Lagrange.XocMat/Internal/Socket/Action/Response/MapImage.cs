using ProtoBuf;


namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class MapImage : BaseActionResponse
{
    [ProtoMember(8)] public byte[] Buffer { get; set; } = Array.Empty<byte>();
}

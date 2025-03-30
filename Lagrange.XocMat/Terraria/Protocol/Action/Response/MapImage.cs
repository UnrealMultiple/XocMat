using ProtoBuf;


namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class MapImage : BaseActionResponse
{
    [ProtoMember(8)] public byte[] Buffer { get; set; } = Array.Empty<byte>();
}

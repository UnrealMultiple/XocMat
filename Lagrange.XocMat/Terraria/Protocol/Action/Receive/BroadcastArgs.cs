using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;


[ProtoContract]
[ProtoInclude(501, typeof(PrivatMsgArgs))]
public class BroadcastArgs : BaseAction
{
    [ProtoMember(5)] public string Text { get; set; } = string.Empty;

    [ProtoMember(6)] public byte[] Color { get; set; } = Array.Empty<byte>();

}

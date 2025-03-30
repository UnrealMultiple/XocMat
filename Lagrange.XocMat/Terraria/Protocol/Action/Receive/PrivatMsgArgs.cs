using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class PrivatMsgArgs : BroadcastArgs
{
    [ProtoMember(7)] public string Name { get; set; } = string.Empty;
}

using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Receive;

[ProtoContract]
public class PrivatMsgArgs : BroadcastArgs
{
    [ProtoMember(7)] public string Name { get; set; }
}

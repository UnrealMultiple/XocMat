using Lagrange.XocMat.Enumerates;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class SocketConnectStatusArgs : BaseAction
{
    [ProtoMember(5)] public SocketConnentType Status { get; set; }
}

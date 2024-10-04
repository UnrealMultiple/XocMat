using Lagrange.XocMat.Internal.Socket.Action;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Receive;

[ProtoContract]
public class QueryAccountArgs : BaseAction
{
    [ProtoMember(5)] public string? Target { get; set; }
}

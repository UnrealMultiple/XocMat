using Lagrange.XocMat.Internal.Socket.Action;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Receive;

[ProtoContract]
public class QueryPlayerInventoryArgs : BaseAction
{
    [ProtoMember(5)] public string Name { get; set; }
}

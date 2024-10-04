using Lagrange.XocMat.Internal.Socket.Action;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Receive;

[ProtoContract]
public class ExportPlayerArgs : BaseAction
{
    [ProtoMember(5)] public List<string> Names { get; set; } = [];
}

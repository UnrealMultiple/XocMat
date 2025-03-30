using Lagrange.XocMat.Terraria.Protocol.Action;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class ExportPlayerArgs : BaseAction
{
    [ProtoMember(5)] public List<string> Names { get; set; } = [];
}

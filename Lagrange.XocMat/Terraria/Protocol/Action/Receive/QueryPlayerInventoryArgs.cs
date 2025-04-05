using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class QueryPlayerInventoryArgs : BaseAction
{
    [ProtoMember(5)] public string Name { get; set; } = string.Empty;
}

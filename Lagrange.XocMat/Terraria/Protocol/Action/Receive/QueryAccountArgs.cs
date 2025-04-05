using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class QueryAccountArgs : BaseAction
{
    [ProtoMember(5)] public string? Target { get; set; }
}

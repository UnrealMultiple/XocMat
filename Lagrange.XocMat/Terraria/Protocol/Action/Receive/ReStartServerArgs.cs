using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class ReStartServerArgs : BaseAction
{
    [ProtoMember(1)] public string StartArgs { get; set; } = string.Empty;
}

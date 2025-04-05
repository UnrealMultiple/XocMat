using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class ServerCommandArgs : BaseAction
{
    [ProtoMember(5)] public string Text { get; set; } = string.Empty;
}

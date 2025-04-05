using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class RegisterAccountArgs : BaseAction
{
    [ProtoMember(5)] public string Name { get; set; } = string.Empty;

    [ProtoMember(6)] public string Group { get; set; } = string.Empty;

    [ProtoMember(7)] public string Password { get; set; } = string.Empty;
}

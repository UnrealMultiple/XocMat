using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Internet;

[ProtoContract]
public class Account
{
    [ProtoMember(1)] public string Name { get; set; } = string.Empty;

    [ProtoMember(2)] public string IP { get; set; } = string.Empty;

    [ProtoMember(3)] public int ID { get; set; }

    [ProtoMember(4)] public string Group { get; set; } = string.Empty;

    [ProtoMember(5)] public string UUID { get; set; } = string.Empty;

    [ProtoMember(6)] public string Password { get; set; } = string.Empty;

    [ProtoMember(7)] public string RegisterTime { get; set; } = string.Empty;

    [ProtoMember(8)] public string LastLoginTime { get; set; } = string.Empty;

}

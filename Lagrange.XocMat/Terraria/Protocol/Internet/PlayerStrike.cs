using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Internet;

[ProtoContract]
public class PlayerStrike
{
    [ProtoMember(1)] public string Player { get; set; } = string.Empty;

    [ProtoMember(2)] public int Damage { get; set; }
}

using Lagrange.XocMat.Terraria.Protocol.Internet;
using ProtoBuf;


namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class PlayerStrikeBoss : BaseActionResponse
{
    [ProtoMember(8)] public List<KillNpc> Damages { get; set; } = [];
}

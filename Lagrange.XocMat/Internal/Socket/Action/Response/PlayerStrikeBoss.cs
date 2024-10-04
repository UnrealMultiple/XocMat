using Lagrange.XocMat.Internal.Socket.Internet;
using ProtoBuf;


namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class PlayerStrikeBoss : BaseActionResponse
{
    [ProtoMember(8)] public List<KillNpc> Damages { get; set; }
}

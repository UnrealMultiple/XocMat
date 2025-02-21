using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class DeadRank : BaseActionResponse
{
    [ProtoMember(8)] public Dictionary<string, int> Rank { get; set; } = [];
}
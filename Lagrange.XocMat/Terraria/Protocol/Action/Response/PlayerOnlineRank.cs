using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class PlayerOnlineRank : BaseActionResponse
{
    [ProtoMember(8)] public Dictionary<string, int> OnlineRank { get; set; } = [];
}

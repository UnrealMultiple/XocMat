using Lagrange.XocMat.Terraria.Protocol.Internet;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class ServerOnline : BaseActionResponse
{
    [ProtoMember(8)] public List<PlayerInfo> Players { get; set; } = [];

    [ProtoMember(9)] public int MaxCount { get; set; }

    [ProtoMember(10)] public int OnlineCount { get; set; }
}

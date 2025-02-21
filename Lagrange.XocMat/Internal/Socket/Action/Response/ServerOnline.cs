using Lagrange.XocMat.Internal.Socket.Internet;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class ServerOnline : BaseActionResponse
{
    [ProtoMember(8)] public List<PlayerInfo> Players { get; set; } = [];

    [ProtoMember(9)] public int MaxCount { get; set; }

    [ProtoMember(10)] public int OnlineCount { get; set; }
}

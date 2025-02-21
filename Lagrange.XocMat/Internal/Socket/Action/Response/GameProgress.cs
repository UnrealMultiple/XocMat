using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class GameProgress : BaseActionResponse
{
    [ProtoMember(8)] public Dictionary<string, bool> Progress { get; set; } = [];
}

using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class GameProgress : BaseActionResponse
{
    [ProtoMember(8)] public Dictionary<string, bool> Progress { get; set; } = [];
}

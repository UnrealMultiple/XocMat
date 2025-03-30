using Lagrange.XocMat.Terraria.Protocol.Internet;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class ExportPlayer : BaseActionResponse
{
    [ProtoMember(8)] public List<PlayerFile> PlayerFiles { get; set; } = [];
}

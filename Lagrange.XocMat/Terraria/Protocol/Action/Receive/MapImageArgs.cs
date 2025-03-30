using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Terraria.Protocol.Action;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class MapImageArgs : BaseAction
{
    [ProtoMember(5)] public ImageType ImageType { get; set; }
}

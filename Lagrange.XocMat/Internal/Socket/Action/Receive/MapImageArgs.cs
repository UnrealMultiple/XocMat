using Lagrange.XocMat.Enumerates;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Receive;

[ProtoContract]
public class MapImageArgs : BaseAction
{
    [ProtoMember(5)] public ImageType ImageType { get; set; }
}

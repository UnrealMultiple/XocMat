using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class ServerCommand : BaseActionResponse
{
    [ProtoMember(8)] public List<string> Params { get; set; } = [];

}

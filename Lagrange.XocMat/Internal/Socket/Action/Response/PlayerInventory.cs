using Lagrange.XocMat.Internal.Socket.Internet;
using ProtoBuf;
namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class PlayerInventory : BaseActionResponse
{
    [ProtoMember(8)] public PlayerData? PlayerData { get; set; }

}

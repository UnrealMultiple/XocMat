using Lagrange.XocMat.Terraria.Protocol.Internet;
using ProtoBuf;
namespace Lagrange.XocMat.Terraria.Protocol.Action.Response;

[ProtoContract]
public class PlayerInventory : BaseActionResponse
{
    [ProtoMember(8)] public PlayerData? PlayerData { get; set; }

}

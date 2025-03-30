using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Internet;


[ProtoContract]
public class Item
{
    [ProtoMember(1)] public int netID { get; set; }

    [ProtoMember(2)] public int prefix { get; set; }

    [ProtoMember(3)] public int stack { get; set; }

    [ProtoMember(4)] public string Name { get; set; } = string.Empty;
}

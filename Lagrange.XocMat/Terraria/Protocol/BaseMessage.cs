using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Terraria.Protocol.Action;
using Lagrange.XocMat.Terraria.Protocol.PlayerMessage;
using Lagrange.XocMat.Terraria.Protocol.ServerMessage;
using ProtoBuf;
using System.Text.Json.Serialization;

namespace Lagrange.XocMat.Terraria.Protocol;

[ProtoContract]
[ProtoInclude(101, typeof(BasePlayerMessage))]
[ProtoInclude(102, typeof(BaseAction))]
[ProtoInclude(103, typeof(GameInitMessage))]
public class BaseMessage
{
    [ProtoMember(1)] public PostMessageType MessageType { get; set; } = PostMessageType.Action;

    [ProtoMember(2)] public string ServerName { get; set; } = string.Empty;

    [ProtoMember(3)] public string Token { get; set; } = string.Empty;

    [JsonIgnore]
    public bool Handler { get; set; }

    [JsonIgnore]
    public TerrariaServer? TerrariaServer => XocMatSetting.Instance.GetServer(ServerName);
}

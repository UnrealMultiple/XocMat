using System.Text.Json.Serialization;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Internal.Socket.Action;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Lagrange.XocMat.Internal.Socket.ServerMessage;
using Lagrange.XocMat.Terraria;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket;

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

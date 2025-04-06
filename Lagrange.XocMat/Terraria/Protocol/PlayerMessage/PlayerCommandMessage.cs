using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.PlayerMessage;

[ProtoContract]
public class PlayerCommandMessage : BasePlayerMessage
{
    [ProtoMember(8)] public string Command { get; set; } = string.Empty;

    [ProtoMember(9)] public string CommandPrefix { get; set; } = string.Empty;

    public TerrariaServer? Server => XocMatSetting.Instance.GetServer(ServerName);

    public TerrariaUser? User => TerrariaUser.GetUsersByName(Name, ServerName);

    public Account Account => Account.GetAccountNullDefault(User?.Id ?? 0);

    public bool HasServerUser => Server != null && User != null;
}

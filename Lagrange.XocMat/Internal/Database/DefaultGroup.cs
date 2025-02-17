using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Permission;

namespace Lagrange.XocMat.Internal.Database;

public class DefaultGroup : Group
{
    public List<string> Selfpermissions =
    [
        OneBotPermissions.Sign,
        OneBotPermissions.Help,
        OneBotPermissions.Jrrp,
        OneBotPermissions.CurrencyUse,
        OneBotPermissions.Nbnhhsh,
        OneBotPermissions.QueryOnlienPlayer,
        OneBotPermissions.QueryProgress,
        OneBotPermissions.QueryInventory,
        OneBotPermissions.ChangeServer,
        OneBotPermissions.QueryUserList,
        OneBotPermissions.GenerateMap,
        OneBotPermissions.ServerList,
        OneBotPermissions.RegisterUser,
        OneBotPermissions.OnlineRank,
        OneBotPermissions.DeathRank,
        OneBotPermissions.SelfInfo,
        OneBotPermissions.TerrariaWiki,
        OneBotPermissions.Version,
        OneBotPermissions.Music,
        OneBotPermissions.EmojiLike,
        OneBotPermissions.TerrariaShop,
        OneBotPermissions.TerrariaPrize,
        OneBotPermissions.ImageEmoji,
        OneBotPermissions.SelfPassword,
        OneBotPermissions.SearchItem,
        OneBotPermissions.KillRank
    ];

    public readonly static DefaultGroup Instance = new();
    public override void NegatePermission(string permission)
    {
        base.NegatePermission(permission);
    }
    public override void RemovePermission(string permission)
    {
        base.RemovePermission(permission);
    }

    public override bool HasPermission(string permission)
    {
        return base.HasPermission(permission);
    }
    public override void AddPermission(string permission)
    {
        base.AddPermission(permission);
    }
    public DefaultGroup() : base(XocMatSetting.Instance.DefaultPermGroup)
    {
        if (Selfpermissions.Count == 0)
            SetPermission(Selfpermissions);
    }
}

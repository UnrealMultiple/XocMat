using Lagrange.XocMat.Entity.Database;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Exceptions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Lagrange.XocMat.DB.Manager;

[Table("Account")]
public class Account : RecordBase<Account>
{
    [Column("ID")]
    [PrimaryKey]
    public long UserId { get; init; }

    [Column("Group")]
    public string GroupName { get; set; } = string.Empty;

    [NotColumn]
    public Group Group
    {
        get
        {
            return Group.GetGroupNullDefault(GroupName);
        }
        set
        {
            GroupName = value.Name;
        }
    }

    private static Context context => Db.Context<Account>("Account");

    public static List<Account> Accounts => context.Records.ToList();

    public bool HasPermission(string perm)
    {
        UserPermissionType result = OperatHandler.UserPermission(this, perm);
        return result switch
        {
            UserPermissionType.Unhandled => false,
            UserPermissionType.Granted => true,
            _ => Group.HasPermission(perm)
        };
    }



    /// <summary>
    /// 查找是否有指定账户
    /// </summary>
    /// <param name="userid"></param>
    /// <param name="groupid"></param>
    /// <returns></returns>
    public static bool HasAccount(long userid)
    {
        return context.Records.Any(x => x.UserId == userid);
    }

    /// <summary>
    /// 添加账户
    /// </summary>
    /// <param name="userid"></param>
    /// <param name="groupid"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static void AddAccount(long userid, string group)
    {
        if (HasAccount(userid))
            throw new AccountException($"账户 {userid} 已经存在了，无法重复添加!");
        if (!Group.HasGroup(group))
            throw new AccountException($"组 {group} 不存在，无法添加!");
        int exec = context.Insert(new Account()
        {
            UserId = userid,
            GroupName = group
        });

        if (exec != 1)
        {
            throw new AccountException($"添加至数据库失败!");
        }
    }

    public static bool HasPermssion(long userid, string perm)
    {
        return GetAccountNullDefault(userid).HasPermission(perm);
    }


    public static Account GetAccountNullDefault(long userid)
    {
        return context.Records.FirstOrDefault(x => x.UserId == userid)
            ?? new Account() { UserId = userid, Group = DefaultGroup.Instance };
    }

    public static Account? GetAccount(long userid)
    {
        return context.Records.FirstOrDefault(x => x.UserId == userid);
    }

    public static bool TryGetAccount(long userid, out Account? account)
    {
        account = GetAccount(userid);
        return account is not null;
    }

    /// <summary>
    /// 更改账户组
    /// </summary>
    /// <param name="userid"></param>
    /// <param name="Group"></param>
    /// <returns></returns>
    public static void ReAccountGroup(long userid, string group)
    {
        if (!HasAccount(userid))
            throw new AccountException($"账户 {userid} 不存在无法更改组!");
        Account account = GetAccountNullDefault(userid);
        account.Group = Group.GetGroupNullDefault(group);
        context.Update(account);

    }

    /// <summary>
    /// 移除账户
    /// </summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    public static void RemoveAccount(long userid)
    {
        if (!HasAccount(userid))
            throw new AccountException($"账户 {userid} 不存在，无法移除!");
        context.Records.Delete(f => f.UserId == userid);
    }
}

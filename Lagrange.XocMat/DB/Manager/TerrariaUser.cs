using Lagrange.XocMat.Exceptions;
using LinqToDB;
using LinqToDB.Mapping;
using System.Data;

namespace Lagrange.XocMat.DB.Manager;

[Table("User")]
public class TerrariaUser : RecordBase<TerrariaUser>
{
    [Column("ID")]
    public long Id { get; init; }

    [Column("Name")]
    public string Name { get; init; } = string.Empty;

    [Column("Server")]
    public string Server { get; init; } = string.Empty;

    [Column("Password")]
    public string Password { get; set; } = string.Empty;

    [Column("GroupID")]
    public long GroupID { get; init; }

    [PrimaryKey, Identity]
    [Column("index")]
    public int Index { get; init; }

    private static Context context => Db.Context<TerrariaUser>("User");

    public static bool HasUser(string server, string Name)
    {
        return context.Records.Any(x => x.Name == Name && x.Server == server);
    }

    public static void Add(long id, long groupid, string Server, string Name, string Password)
    {
        if (context.Records.Any(x => x.Id == id && x.Name == Name && x.Server == Server))
            throw new TerrariaUserException("此用户已经注册过了!");
        //搜索名字和服务器
        TerrariaUser? user = GetUsersByName(Name, Server);
        if (user != null)
            throw new TerrariaUserException($"此名称已经被{user.Id}注册过了!");
        context.Insert(new TerrariaUser()
        {
            Id = id,
            Server = Server,
            Password = Password,
            Name = Name,
            GroupID = groupid
        });

    }
    public static void ResetPassword(long id, string servername, string name, string pwd)
    {
        TerrariaUser user = GetUserById(id, servername, name) ?? throw new GroupException("删除权限指向的目标组不存在!");
        user.Password = pwd;
        context.Update(user);
    }

    public static void Remove(string Server, string Name)
    {
        TerrariaUser? user = GetUsersByName(Name, Server);
        if (user == null)
            throw new TerrariaUserException($"在{Server} 上没有找到{Name}");
        context.Records.Delete(i => i.Server == Server && i.Name == Name);
    }

    public static void RemoveByServer(string server)
    {
        context.Records.Delete(i => i.Server == server);
    }

    public static List<TerrariaUser> GetUsers(string Server)
    {
        return [.. context.Records.Where(f => f.Server == Server)];
    }

    public static List<TerrariaUser> GetUsers(long id)
    {
        return [.. context.Records.Where(f => f.Id == id)];
    }

    public static List<TerrariaUser> GetUserById(long id, string server)
    {
        return [.. context.Records.Where(f => f.Server == server && f.Id == id)];
    }

    public static TerrariaUser? GetUserById(long id, string server, string name)
    {
        return context.Records.FirstOrDefault(f => f.Server == server && f.Name == name && f.Id == id);
    }

    public static TerrariaUser? GetUsersByName(string name, string server)
    {
        return context.Records.FirstOrDefault(x => x.Name == name && x.Server == server);
    }

    public static TerrariaUser? GetUsersByName(string name)
    {
        return context.Records.FirstOrDefault(x => x.Name == name);
    }

    public static void Reset()
    {
        context.Records.Delete();
    }
}

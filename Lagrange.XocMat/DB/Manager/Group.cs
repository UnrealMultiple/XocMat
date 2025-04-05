using System.Data;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Entity.Database;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Lagrange.XocMat.DB.Manager;

[Table("GroupList")]
public class Group : RecordBase<Group>
{
    [Column]
    [PrimaryKey]
    public string Name { get; set; } = string.Empty;

    [Column]
    public string Permission
    {
        get
        {
            List<string> all = new(permissions);
            negatedpermissions.ForEach(p => all.Add("!" + p));
            return string.Join(",", all);
        }
        set
        {
            permissions.Clear();
            negatedpermissions.Clear();
            value?.Split(',').ForEach(p => AddPermission(p.Trim()));
        }
    }

    [Column]
    public string parent { get; set; } = string.Empty;


    private static Context context => Db.Context<Group>("GroupList");

    private List<string> negatedpermissions { get; set; } = [];

    [NotColumn]
    public List<string> permissions { get; set; } = [];

    public Group? Parent
    {
        get => GetGroup(parent);
        set
        {
            parent = value?.Name ?? string.Empty;
        }
    }

    public Group()
    {

    }

    public Group(string name)
    {
        Name = name;

    }


    public virtual void AddPermission(string permission)
    {
        if (permission.StartsWith("!"))
        {
            NegatePermission(permission[1..]);
            return;
        }

        if (!permissions.Contains(permission))
        {
            permissions.Add(permission);
            negatedpermissions.Remove(permission);
        }
    }

    public virtual void NegatePermission(string permission)
    {
        // Avoid duplicates
        if (!negatedpermissions.Contains(permission))
        {
            negatedpermissions.Add(permission);
            permissions.Remove(permission); // Ensure we don't have conflicting definitions for a permissions
        }
    }

    public void SetPermission(List<string> permission)
    {
        permissions.Clear();
        negatedpermissions.Clear();
        permission.ForEach(p => AddPermission(p));
    }

    public virtual void RemovePermission(string permission)
    {
        if (permission.StartsWith("!"))
        {
            negatedpermissions.Remove(permission.Substring(1));
            return;
        }
        permissions.Remove(permission);
    }

    public virtual List<string> TotalPermissions
    {
        get
        {
            Group? cur = this;
            List<Group> traversed = [];
            HashSet<string> all = [];
            while (cur != null)
            {
                foreach (string perm in cur.permissions)
                {
                    all.Add(perm);
                }

                foreach (string perm in cur.negatedpermissions)
                {
                    all.Remove(perm);
                }

                if (traversed.Contains(cur))
                {
                    throw new Exception("Infinite group parenting ({0})".SFormat(cur.Name));
                }
                traversed.Add(cur);
                cur = cur.Parent;
            }
            return all.ToList();
        }
    }

    public virtual bool HasPermission(string permission)
    {
        bool negated = false;
        if (string.IsNullOrEmpty(permission) || (RealHasPermission(permission, ref negated) && !negated))
        {
            return true;
        }

        if (negated)
            return false;

        string[] nodes = permission.Split('.');
        for (int i = nodes.Length - 1; i >= 0; i--)
        {
            nodes[i] = "*";
            if (RealHasPermission(string.Join(".", nodes, 0, i + 1), ref negated))
            {
                return !negated;
            }
        }
        return false;
    }

    private bool RealHasPermission(string permission, ref bool negated)
    {
        negated = false;
        if (string.IsNullOrEmpty(permission))
            return true;

        Group? cur = this;
        List<Group> traversed = [];
        while (cur != null)
        {
            if (cur.negatedpermissions.Contains(permission))
            {
                negated = true;
                return false;
            }
            if (cur.permissions.Contains(permission))
                return true;
            if (traversed.Contains(cur))
            {
                throw new InvalidOperationException("Infinite group parenting ({0})".SFormat(cur.Name));
            }
            traversed.Add(cur);
            cur = cur.Parent;
        }
        return false;
    }

    public static List<Group> GetGroups() => [.. context.Records];


    public static void AddGroup(string groupName, string perms = "")
    {
        if (context.Records.Any(i => i.Name == groupName))
            if (HasGroup(groupName))
            {
                throw new GroupException("此组已经存在了，无法重复添加!");
            }
        int exec = context.Insert(new Group()
        {
            Name = groupName,
            Permission = perms,
            parent = XocMatSetting.Instance.DefaultPermGroup
        });
        if (exec != 1)
            throw new GroupException("添加至数据库失败!");
    }

    public static void AddPerm(string groupName, string perm)
    {
        Group group = GetGroup(groupName) ?? throw new GroupException($"组 {groupName} 不存在!");
        if (!group.permissions.Contains(perm))
        {
            group.AddPermission(perm);
            context.Update(group);
        }
        else
        {
            throw new GroupException("权限已存在请不要重复添加!!");
        }
    }

    public static void ReParentGroup(string groupName, string Parent)
    {
        Group group = GetGroup(groupName) ?? throw new GroupException($"组 {groupName} 不存在!");
        group.Parent = GetGroupNullDefault(Parent);
        context.Update(group);
    }


    public static bool HasGroup(string? Name)
    {
        return context.Records.Any(i => i.Name == Name);
    }

    public static void RemovePerm(string groupName, string perm)
    {
        Group group = GetGroup(groupName) ?? throw new GroupException("删除权限指向的目标组不存在!");
        if (group.permissions.Contains(perm))
        {
            group.RemovePermission(perm);
            context.Update(group);
        }
        else
        {
            throw new GroupException("此组没有该权限无需删除!");
        }
    }

    public static void RemoveGroup(string groupName)
    {
        if (!HasGroup(groupName))
            throw new GroupException($"组 {groupName} 不存在!");
        if (context.Records.Delete(x => x.Name == groupName) != 1)
        {
            throw new GroupException("更新至数据库失败!");
        }
    }

    public static List<string> GetGroupPerms(string groupName)
    {
        return context.Records.FirstOrDefault(i => i.Name == groupName)?.permissions ?? [];
    }

    public static bool HasPermssions(string groupName, string perm)
    {
        return context.Records.Where(x => x.Name == groupName).Any(x => x.permissions.Contains(perm));
    }

    public static Group? GetGroup(string groupName)
    {
        return context.Records.FirstOrDefault(i => i.Name == groupName);
    }

    public static Group GetGroupNullDefault(string groupName)
    {
        return GetGroup(groupName)
            ?? DefaultGroup.Instance;
    }
}

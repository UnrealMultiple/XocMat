using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class GroupManagerCommand : Command
{
    public override string[] Alias => ["group"];
    public override string HelpText => "群管理命令";
    public override string[] Permissions => [OneBotPermissions.Group];

    private static readonly Dictionary<string, Func<GroupCommandArgs, ILogger, Task>> _action = new()
    {
        { "add", Add },
        { "del", Del },
        { "addperm", AddPerm },
        { "delperm", DelPerm },
        { "parent", Parent },
        { "list", GroupList }
    };

    private static async Task GroupList(GroupCommandArgs args, ILogger logger)
    {
        var builder = TableBuilder.Create()
            .SetTitle("权限组列表")
            .SetMemberUin(args.MemberUin)
            .SetHeader("组名", "权限");
        var groups = DB.Manager.Group.GetGroups();
        if (groups.Count == 0)
        {
            await args.Event.Reply("还没有添加任何权限组!", true);
            return;
        }
        foreach (var group in groups)
        {
            builder.AddRow(group.Name, group.permissions.JoinToString(","));
        }
        await args.MessageBuilder.Image(builder.Builder()).Reply();
    }

    private static async Task Parent(GroupCommandArgs args, ILogger logger)
    {
        if (args.Parameters.Count != 3)
        {
            await args.Event.Reply("参数错误，数量不正确!", true);
            return;
        }
        try
        {
            DB.Manager.Group.ReParentGroup(args.Parameters[1], args.Parameters[2]);
            await args.Event.Reply($"组 {args.Parameters[1]} 的组已更改为 {args.Parameters[2]}!", true);
        }
        catch (Exception ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    private static async Task DelPerm(GroupCommandArgs args, ILogger logger)
    {
        if (args.Parameters.Count != 3)
        {
            await args.Event.Reply("参数错误，数量不正确!", true);
            return;
        }
        try
        {
            DB.Manager.Group.RemovePerm(args.Parameters[1], args.Parameters[2]);
            await args.Event.Reply($"权限删除成功!", true);
        }
        catch (GroupException ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    private static async Task AddPerm(GroupCommandArgs args, ILogger logger)
    {
        if (args.Parameters.Count != 3)
        {
            await args.Event.Reply("参数错误，数量不正确!", true);
            return;
        }
        try
        {
            DB.Manager.Group.AddPerm(args.Parameters[1], args.Parameters[2]);
            await args.Event.Reply($"权限添加成功!", true);
        }
        catch (GroupException ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    private static async Task Del(GroupCommandArgs args, ILogger logger)
    {
        if (args.Parameters.Count != 2)
        {
            await args.Event.Reply("参数错误，数量不正确!", true);
            return;
        }
        try
        {
            DB.Manager.Group.RemoveGroup(args.Parameters[1]);
            await args.Event.Reply($"组 {args.Parameters[1]} 删除成功!", true);
        }
        catch (GroupException ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    private static async Task Add(GroupCommandArgs args, ILogger logger)
    {
        if (args.Parameters.Count != 2)
        {
            await args.Event.Reply("参数错误，数量不正确!", true);
            return;
        }
        try
        {
            DB.Manager.Group.AddGroup(args.Parameters[1]);
            await args.Event.Reply($"组 {args.Parameters[1]} 添加成功!", true);
        }
        catch (GroupException ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count > 0 && _action.TryGetValue(args.Parameters[0], out var action))
        {
            await action(args, log);
        }
        else
        {
            await args.MessageBuilder
                .Text("语法错误，正确语法:\n")
                .Text($"{args.CommandPrefix}group add [组]\n")
                .Text($"{args.CommandPrefix}group del [组]\n")
                .Text($"{args.CommandPrefix}group addperm [组] [权限]\n")
                .Text($"{args.CommandPrefix}group delperm [组] [权限]\n")
                .Text($"{args.CommandPrefix}group parent [组] [父组]")
                .Text($"{args.CommandPrefix}group list")
                .Reply();
        }
    }
}
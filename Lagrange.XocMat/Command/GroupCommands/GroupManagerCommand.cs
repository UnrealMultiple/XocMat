using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class GroupManagerCommand : Command
{
    public override string[] Alias => ["group"];
    public override string HelpText => "群管理命令";
    public override string[] Permissions => [OneBotPermissions.Group];
    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "add")
        {
            try
            {
                DB.Manager.Group.AddGroup(args.Parameters[1]);
                await args.Event.Reply($"组 {args.Parameters[1]} 添加成功!");
            }
            catch (GroupException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "del")
        {
            try
            {
                DB.Manager.Group.RemoveGroup(args.Parameters[1]);
                await args.Event.Reply($"组 {args.Parameters[1]} 删除成功!");
            }
            catch (GroupException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "parent")
        {
            try
            {
                DB.Manager.Group.ReParentGroup(args.Parameters[1], args.Parameters[2]);
                await args.Event.Reply($"组 {args.Parameters[1]} 的组已更改为 {args.Parameters[2]}!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "addperm")
        {
            try
            {
                DB.Manager.Group.AddPerm(args.Parameters[1], args.Parameters[2]);
                await args.Event.Reply($"权限添加成功!");
            }
            catch (GroupException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "delperm")
        {
            try
            {
                DB.Manager.Group.RemovePerm(args.Parameters[1], args.Parameters[2]);
                await args.Event.Reply($"权限删除成功!");
            }
            catch (GroupException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.MessageBuilder
                .Text("语法错误，正确语法:\n")
                .Text($"{args.CommamdPrefix}group add <组>\n")
                .Text($"{args.CommamdPrefix}group del <组>\n")
                .Text($"{args.CommamdPrefix}group addperm <组> <权限>\n")
                .Text($"{args.CommamdPrefix}group delperm <组> <权限>\n")
                .Text($"{args.CommamdPrefix}group parent <组> <父组>")
                .Reply();
        }
    }
}
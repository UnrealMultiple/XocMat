using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class SearchCommandPermission : Command
{
    public override string[] Alias => ["sperm"];
    public override string HelpText => "搜索命令权限";
    public override string[] Permissions => [OneBotPermissions.SearchCommandPerm];
    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1)
        {
            string banName = args.Parameters[0];
            List<string> comm = XocMatAPI.CommandManager.Commands.Where(x => x.Alias.Contains(banName)).SelectMany(x => x.Permissions).ToList();
            if (comm == null || comm.Count == 0)
            {
                await args.Event.Reply("没有找到该指令，无法查询！");
            }
            else
            {
                args.MessageBuilder.Text(banName + "指令的权限可能为:\n");

                comm.ForEach(x => args.MessageBuilder.Text(x));
                await args.MessageBuilder.Reply();
            }
        }
        else
        {
            await args.Event.Reply($"语法错误，正确语法:{args.CommamdPrefix}scmdperm [指令名]");
        }
    }
}

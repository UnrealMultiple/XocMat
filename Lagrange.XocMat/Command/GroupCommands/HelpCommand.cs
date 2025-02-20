using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Utility;


namespace Lagrange.XocMat.Command.InternalCommands;

public class HelpCommand : Command
{
    public override string[] Name => ["help"];

    public override string HelpText => "获取命令列表";

    public override string Permission => OneBotPermissions.Help;

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        void Show(List<string> line)
        {
            if (PaginationTools.TryParsePageNumber(args.Parameters, 0, args.EventArgs, out int page))
            {
                PaginationTools.SendPage(args.EventArgs, page, line, new PaginationTools.Settings()
                {
                    MaxLinesPerPage = 15,
                    NothingToDisplayString = "当前没有指令可用",
                    HeaderFormat = "指令列表 ({0}/{1})：",
                    FooterFormat = $"输入 {args.CommamdPrefix}help {{0}} 查看更多"
                });
            }
        }
        var commands = XocMatAPI.Command!.Commands.Select(x => args.CommamdPrefix + x.Name.First()).ToList();
        Show(commands);
        await ValueTask.CompletedTask;
    }
}

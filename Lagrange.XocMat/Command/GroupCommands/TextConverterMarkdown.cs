using Lagrange.Core.Message.Entity;
using Lagrange.Core.Message;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Command.InternalCommands;

public class TextConverterMarkdown : Command
{
    public override string[] Name => ["md"];

    public override string HelpText => "将文本转换为Markdown格式";

    public override string Permission => "markdown";

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1 && !string.IsNullOrEmpty(args.Parameters[0]))
        {
            var builder = MessageBuilder.Group(args.EventArgs.Chain.GroupUin!.Value)
                .MultiMsg(MessageBuilder.Friend(args.Bot.BotUin)
                .MultiMsg(MessageBuilder.Friend(args.Bot.BotUin)
                .Markdown(new MarkdownData()
                {
                    Content = args.Parameters[0]
                })));

            await args.EventArgs.Reply(builder);
        } 
        else
        {
            await args.EventArgs.Reply("请输入md内容!");
        }
    } 
}

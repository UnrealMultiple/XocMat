using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Lagrange.XocMat.Command.GroupCommands;

public class TextConverterMarkdown : Command
{
    public override string[] Alias => ["md"];

    public override string HelpText => "将文本转换为Markdown格式";

    public override string[] Permissions => [OneBotPermissions.UserAdmin];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1 && !string.IsNullOrEmpty(args.Parameters[0]))
        {
            MessageBuilder builder = MessageBuilder.Group(args.GroupUin)
                .MultiMsg(MessageBuilder.Friend(args.Bot.BotUin)
                .MultiMsg(MessageBuilder.Friend(args.Bot.BotUin)
                .Markdown(new MarkdownData()
                {
                    Content = args.Parameters[0]
                })));

            await args.Event.Reply(builder);
        }
        else
        {
            await args.Event.Reply("请输入md内容!");
        }
    }
}

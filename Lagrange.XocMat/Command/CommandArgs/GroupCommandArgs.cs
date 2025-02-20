using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.DB.Manager;

namespace Lagrange.XocMat.Command.CommandArgs;

public class GroupCommandArgs(BotContext bot, string name, GroupMessageEvent args, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine, Account account)
    : BaseCommandArgs(bot, name, commamdPrefix, parameters, commamdLine)
{
    public GroupMessageEvent EventArgs { get; } = args;

    public Account Account { get; } = account;

    public MessageBuilder MessageBuilder { get; } = MessageBuilder.Group(args.Chain.GroupUin!.Value);   
}

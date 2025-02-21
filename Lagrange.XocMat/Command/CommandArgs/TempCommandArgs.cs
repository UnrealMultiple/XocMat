using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.DB.Manager;

namespace Lagrange.XocMat.Command.CommandArgs;

public class TempCommandArgs(BotContext bot, string name, TempMessageEvent args, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine, Account account)
    : BaseCommandArgs(bot, name, commamdPrefix, parameters, commamdLine)
{
    public TempMessageEvent Event { get; } = args;

    public Account Account { get; } = account;

    public MessageBuilder MessageBuilder { get; } = MessageBuilder.Friend(args.Chain.FriendUin);
}

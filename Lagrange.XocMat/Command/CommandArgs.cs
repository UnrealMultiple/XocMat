using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.XocMat.DB.Manager;

namespace Lagrange.XocMat.Commands;

public class CommandArgs(BotContext bot, string name, GroupMessageEvent args, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine, AccountManager.Account account)
    : BaseCommandArgs(bot, name, commamdPrefix, parameters, commamdLine)
{
    public GroupMessageEvent EventArgs { get; } = args;

    public AccountManager.Account Account { get; } = account;

}

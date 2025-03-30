using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Command.CommandArgs;

public class FriendCommandArgs(BotContext bot, string name, FriendMessageEvent args, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine, Account account)
    : BaseCommandArgs(bot, name, commamdPrefix, parameters, commamdLine)
{
    public FriendMessageEvent Event { get; } = args;

    public Account Account { get; } = account;

    public MessageBuilder MessageBuilder { get; } = MessageBuilder.Friend(args.Chain.FriendUin);

    public override string ToPreviewString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [FriendCommand({Event.Chain.FriendUin})] [{CommandPrefix}{Name}] [Parameters]: {Parameters.JoinToString(",")}";

    public override string ToPerviewErrorString(Exception e) => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [FriendCommand({Event.Chain.FriendUin})] [{CommandPrefix}{Name}] [ErrorText]: {e}";

    public override string ToSkippingString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [FriendCommand({Event.Chain.FriendUin})] [试图越级使用命令]: {CommandPrefix}{Name}";
}

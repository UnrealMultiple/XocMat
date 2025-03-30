using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Command.CommandArgs;

public class GroupCommandArgs : BaseCommandArgs
{
    public GroupCommandArgs(BotContext bot, string name, GroupMessageEvent args, string commamdPrefix, List<string> parameters, Dictionary<string, string> commamdLine, Account account)
        : base(bot, name, commamdPrefix, parameters, commamdLine)
    {
        Event = args;
        Account = account;
        MessageBuilder = MessageBuilder.Group(args.Chain.GroupUin!.Value);
    }

    public GroupMessageEvent Event { get; }

    public Account Account { get; }

    public MessageBuilder MessageBuilder { get; }

    public uint GroupUin => Event.Chain.GroupUin!.Value;

    public uint MemberUin => Event.Chain.GroupMemberInfo!.Uin;

    public string MemberName => Event.Chain.GroupMemberInfo!.MemberName;

    public GroupMemberPermission IsAdmin => Event.Chain.GroupMemberInfo!.Permission;

    public string MemberCard => Event.Chain.GroupMemberInfo!.MemberCard ?? "";

    public string MemberSpecialTitle => Event.Chain.GroupMemberInfo!.SpecialTitle ?? "";

    public override string ToPreviewString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [GroupCommand({GroupUin})({MemberUin})] [{CommandPrefix}{Name}] [Parameters]: {Parameters.JoinToString(",")}";

    public override string ToPerviewErrorString(Exception e) => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [GroupCommand({GroupUin})({MemberUin})] [{CommandPrefix}{Name}] [ErrorText]: {e}";

    public override string ToSkippingString() => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [GroupCommand({GroupUin})({MemberUin})] [试图越级使用命令]: {CommandPrefix}{Name}";
}

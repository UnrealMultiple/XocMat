
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility.Images;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class AccountManagerCommand : Command
{
    public override string[] Alias => ["account"];
    public override string HelpText => "账号管理";
    public override string[] Permissions => [OneBotPermissions.Account];

    private static readonly Dictionary<string, Func<GroupCommandArgs, ILogger, Task>> _action = new()
    {
        { "add", Add },
        { "del", Del },
        { "group", ChangeGroup },
        { "list", AccountList },
    };

    private static async Task AccountList(GroupCommandArgs args, ILogger logger)
    {
        try
        {
            var accounts = DB.Manager.Account.Accounts;
            var builder = ProfileItemBuilder.Create()
                .SetTitle("账户列表")
                .SetMemberUin(args.MemberUin);
            if (accounts.Count == 0)
            {
                await args.Event.Reply("当前没有任何账户!", true);
                return;
            }
            accounts.ForEach(x => builder.AddItem(x.UserId.ToString(), x.GroupName));
            await args.MessageBuilder.Image(builder.Build()).Reply();
        }
        catch (AccountException ex)
        {
            await args.Event.Reply(ex.Message);
        }
    }

    private static async Task ChangeGroup(GroupCommandArgs args, ILogger logger)
    {
        if (VerifyParameters(args, "group", out var uin, out var group))
        {
            try
            {
                DB.Manager.Account.ReAccountGroup(uin, group);
                await args.Event.Reply($"账户 {uin} 的组 成功更改为 {group}");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.Event.Reply("语法错误，请检查后使用!", true);
        }
    }

    private static async Task Del(GroupCommandArgs args, ILogger logger)
    {
        if (VerifyParameters(args, "del", out var uin, out var _))
        {
            try
            {
                DB.Manager.Account.RemoveAccount(uin);
                await args.Event.Reply($"删除成功!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.Event.Reply("语法错误，请检查后使用!", true);
        }
    }

    private static bool VerifyParameters(GroupCommandArgs args, string subcmd, out uint uin, out string group)
    {
        var atList = args.Event.Chain.GetMention();
        uin = args.MemberUin;
        group = XocMatSetting.Instance.DefaultPermGroup;
        if (args.Parameters.Count < 2)
        {
            return false;
        }
        if (args.Parameters.Count == 2 && args.Parameters[0].Equals(subcmd, StringComparison.CurrentCultureIgnoreCase) && atList.Any())
        {
            uin = atList.First().Uin;
            group = args.Parameters[1];

        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == subcmd)
        {
            group = args.Parameters[2];
            if (!uint.TryParse(args.Parameters[1], out uin))
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    private static async Task Add(GroupCommandArgs args, ILogger logger)
    {
        if (VerifyParameters(args, "add", out var uin, out var group))
        {
            try
            {
                DB.Manager.Account.AddAccount(uin, group);
                await args.Event.Reply($"{uin} 已添加到组 {group}", true);
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message, true);
            }
        }
        else
        {
            await args.Event.Reply("语法错误，请检查后使用!", true);
        }
    }

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count > 0 && _action.TryGetValue(args.Parameters[0], out var action))
        {
            await action(args, log);
        }
        else
        {
            await args.MessageBuilder
                .Text("语法错误，正确的语法:\n")
                .Text($"{args.CommandPrefix}account add [组] at\n")
                .Text($"{args.CommandPrefix}account del [组] at\n")
                .Text($"{args.CommandPrefix}account add [QQ] [组]\n")
                .Text($"{args.CommandPrefix}account del [QQ] [组]\n")
                .Text($"{args.CommandPrefix}account group [组] at\n")
                .Text($"{args.CommandPrefix}account group [QQ] [组]\n")
                .Text($"{args.CommandPrefix}account list")
                .Reply();
        }
    }
}
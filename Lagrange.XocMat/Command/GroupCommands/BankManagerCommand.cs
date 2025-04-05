using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class BankManagerCommand : Command
{
    public override string[] Alias => ["bank"];
    public override string HelpText => "银行管理命令";
    public override string[] Permissions => [OneBotPermissions.CurrencyAdmin, OneBotPermissions.CurrencyUse];

    private static readonly Dictionary<string, Func<GroupCommandArgs, ILogger, Task>> _action = new()
    {
        { "add", Add },
        { "del", Del },
        { "pay", Pay },
    };

    private static async Task Pay(GroupCommandArgs args, ILogger logger)
    {
        if (!VerifyParameters(args, "pay", out var uin, out var num))
        {
            await args.Event.Reply("语法错误，请检查参数是否有误!", true);
        }
        else
        {
            var usercurr = DB.Manager.Currency.Query(args.MemberUin);
            if (usercurr == null || usercurr.Num < num)
            {
                await args.Event.Reply($"你没有足够的{XocMatSetting.Instance.Currency}付给他人!");
            }
            else
            {
                try
                {
                    DB.Manager.Currency.Del(args.MemberUin, num);
                    DB.Manager.Currency.Add(uin, num);
                    await args.Event.Reply($"成功付给 {uin}  {num}个{XocMatSetting.Instance.Currency}!");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
        }
    }

    private static async Task Del(GroupCommandArgs args, ILogger logger)
    {
        if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
        {
            await args.Event.Reply("你没有权限执行此命令!", true);
            return;
        }
        if (!VerifyParameters(args, "del", out var uin, out var num))
        {
            await args.Event.Reply("语法错误，请检查参数是否有误!", true);
        }
        else
        {
            try
            {
                DB.Manager.Currency.Del(uin, num);
                await args.Event.Reply($"成功删除 {uin} 的 {num}个{XocMatSetting.Instance.Currency}!", true);
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
    }

    private static async Task Add(GroupCommandArgs args, ILogger logger)
    {
        if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
        {
            await args.Event.Reply("你没有权限执行此命令!", true);
            return;
        }
        if (!VerifyParameters(args, "add", out var uin, out var num))
        {
            await args.Event.Reply("语法错误，请检查参数是否有误!", true);
        }
        else
        {
            try
            {
                DB.Manager.Currency.Add(uin, num);
                await args.Event.Reply($"成功为 {uin} 添加{num}个{XocMatSetting.Instance.Currency}!", true);
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
    }

    private static bool VerifyParameters(GroupCommandArgs args, string subcmd, out uint uin, out int num)
    {
        var atList = args.Event.Chain.GetMention();
        uin = args.MemberUin;
        num = 0;
        if (args.Parameters.Count < 2)
        {
            return false;
        }
        if (args.Parameters.Count == 2 && args.Parameters[0].Equals(subcmd, StringComparison.CurrentCultureIgnoreCase) && atList.Any())
        {
            uin = atList.First().Uin;
            if (!int.TryParse(args.Parameters[1], out num))
            {
                return false;
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == subcmd)
        {

            if (!int.TryParse(args.Parameters[2], out num) || !uint.TryParse(args.Parameters[1], out uin))
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

    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        if (args.Parameters.Count > 0 && _action.TryGetValue(args.Parameters[0], out var action))
        {
            await action(args, log);
        }
        else
        {
            await args.Event.Reply("语法错误，正确语法:\n" +
                $"{args.CommandPrefix}bank add [qq] [数量]\n" +
                $"{args.CommandPrefix}bank add [数量] at\n" +
                $"{args.CommandPrefix}bank del [qq] [数量]\n" +
                $"{args.CommandPrefix}bank del [数量] at\n" +
                $"{args.CommandPrefix}bank pay [qq] 数量\n" +
                $"{args.CommandPrefix}bank pay [数量] at");
        }
    }
}

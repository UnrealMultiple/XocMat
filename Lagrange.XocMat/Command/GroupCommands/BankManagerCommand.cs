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
    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        IEnumerable<Core.Message.Entity.MentionEntity> at = args.Event.Chain.GetMention();
        if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "add")
        {
            if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
            {
                await args.Event.Reply("你没有权限执行此命令!");
                return;
            }
            if (!long.TryParse(args.Parameters[1], out long qq))
            {
                await args.Event.Reply("错误得QQ账号，无法转换为数值!");
                return;
            }

            if (!long.TryParse(args.Parameters[2], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            try
            {
                DB.Manager.Currency result = DB.Manager.Currency.Add(qq, num);
                await args.Event.Reply($"成功为 {qq} 添加{num}个{XocMatSetting.Instance.Currency}!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }

        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "add" && at.Count() == 1)
        {
            if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
            {
                await args.Event.Reply("你没有权限执行此命令!");
                return;
            }
            if (!long.TryParse(args.Parameters[1], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            try
            {
                DB.Manager.Currency result = DB.Manager.Currency.Add(at.First().Uin, num);
                await args.Event.Reply($"成功为 {at.First().Name} 添加{num}个{XocMatSetting.Instance.Currency}!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "del")
        {
            if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
            {
                await args.Event.Reply("你没有权限执行此命令!");
                return;
            }
            if (!long.TryParse(args.Parameters[1], out long qq))
            {
                await args.Event.Reply("错误得QQ账号，无法转换为数值!");
                return;
            }

            if (!long.TryParse(args.Parameters[2], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            try
            {
                DB.Manager.Currency? result = DB.Manager.Currency.Del(qq, num);
                await args.Event.Reply($"成功删除 {qq} 的 {num}个{XocMatSetting.Instance.Currency}!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }

        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "del" && at.Count() == 1)
        {
            if (!args.Account.HasPermission(OneBotPermissions.CurrencyAdmin))
            {
                await args.Event.Reply("你没有权限执行此命令!");
                return;
            }
            if (!long.TryParse(args.Parameters[1], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            try
            {
                DB.Manager.Currency? result = DB.Manager.Currency.Del(at.First().Uin, num);
                await args.Event.Reply($"成功扣除 {at.First().Name} 的 {num}个{XocMatSetting.Instance.Currency}!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "pay")
        {
            if (!long.TryParse(args.Parameters[1], out long qq))
            {
                await args.Event.Reply("错误得QQ账号，无法转换为数值!");
                return;
            }

            if (!long.TryParse(args.Parameters[2], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            DB.Manager.Currency? usercurr = DB.Manager.Currency.Query(args.MemberUin);
            if (usercurr == null || usercurr.Num < num)
            {
                await args.Event.Reply($"你没有足够的{XocMatSetting.Instance.Currency}付给他人!");
            }
            else
            {
                try
                {
                    DB.Manager.Currency.Del(args.MemberUin, num);
                    DB.Manager.Currency.Add(qq, num);
                    await args.Event.Reply($"成功付给 {qq}  {num}个{XocMatSetting.Instance.Currency}!");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "pay" && at.Count() == 1)
        {
            if (!long.TryParse(args.Parameters[1], out long num))
            {
                await args.Event.Reply("错误得数量，无法转换为数值!");
                return;
            }
            DB.Manager.Currency? usercurr = DB.Manager.Currency.Query(args.MemberUin);
            if (usercurr == null || usercurr.Num < num)
            {
                await args.Event.Reply($"你没有足够的{XocMatSetting.Instance.Currency}付给他人!");
            }
            else
            {
                try
                {
                    DB.Manager.Currency.Del(args.MemberUin, num);
                    DB.Manager.Currency.Add(at.First().Uin, num);
                    await args.Event.Reply($"成功付给 {at.First().Name}  {num}个{XocMatSetting.Instance.Currency}!");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
        }
        else
        {
            await args.Event.Reply("语法错误，正确语法:\n" +
                $"{args.CommamdPrefix}bank add <qq> <数量>\n" +
                $"{args.CommamdPrefix}bank add <数量> at\n" +
                $"{args.CommamdPrefix}bank del <qq> <数量>\n" +
                $"{args.CommamdPrefix}bank del <数量> at\n" +
                $"{args.CommamdPrefix}bank pay <qq> 数量\n" +
                $"{args.CommamdPrefix}bank pay <数量> at");
        }
    }
}

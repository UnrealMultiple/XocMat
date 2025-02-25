using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Exceptions;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command.GroupCommands;

public class AccountManagerCommand : Command
{
    public override string[] Alias => ["account", "accountmanager"];
    public override string HelpText => "账号管理";
    public override string[] Permissions => [OneBotPermissions.Account];
    public override async Task InvokeAsync(GroupCommandArgs args, ILogger log)
    {
        void Show(List<string> line)
        {
            if (PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Event, out int page))
            {
                PaginationTools.SendPage(args.Event, page, line, new PaginationTools.Settings()
                {
                    MaxLinesPerPage = 6,
                    NothingToDisplayString = "当前没有账户",
                    HeaderFormat = "账户列表 ({0}/{1})：",
                    FooterFormat = $"输入 {args.CommamdPrefix}account list {{0}} 查看更多"
                });
            }
        }
        IEnumerable<Core.Message.Entity.MentionEntity> atList = args.Event.Chain.GetMention();
        if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "add" && atList.Any())
        {
            try
            {
                DB.Manager.Account.AddAccount(atList.First().Uin, args.Parameters[1]);
                await args.Event.Reply($"{atList.First().Uin} 已添加到组 {args.Parameters[1]}");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "add")
        {
            if (long.TryParse(args.Parameters[1], out long id))
            {
                try
                {
                    DB.Manager.Account.AddAccount(id, args.Parameters[2]);
                    await args.Event.Reply($"{id} 已添加到组 {args.Parameters[2]}");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
            else
            {
                await args.Event.Reply("错误的QQ账号，无法转换!");
            }
        }
        else if (args.Parameters.Count == 1 && args.Parameters[0].ToLower() == "del" && args.Event.Chain.GetMention().Any())
        {
            try
            {
                DB.Manager.Account.RemoveAccount(atList.First().Uin);
                await args.Event.Reply($"删除成功!");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "del")
        {
            if (long.TryParse(args.Parameters[1], out long id))
            {
                try
                {
                    DB.Manager.Account.RemoveAccount(id);
                    await args.Event.Reply($"删除成功!");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
            else
            {
                await args.Event.Reply("错误的QQ账号，无法转换!");
            }
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "group" && atList.Any())
        {
            try
            {
                DB.Manager.Account.ReAccountGroup(atList.First().Uin, args.Parameters[1]);
                await args.Event.Reply($"账户 {atList.First().Uin} 的组 成功更改为 {args.Parameters[1]}");
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "group")
        {
            if (long.TryParse(args.Parameters[1], out long id))
            {
                try
                {
                    DB.Manager.Account.ReAccountGroup(id, args.Parameters[1]);
                    await args.Event.Reply($"账户 {id} 的组 成功更改为 {args.Parameters[1]}");
                }
                catch (Exception ex)
                {
                    await args.Event.Reply(ex.Message);
                }
            }
            else
            {
                await args.Event.Reply("错误的QQ账号，无法转换!");
            }
        }
        else if (args.Parameters.Count >= 1 && args.Parameters[0].ToLower() == "list")
        {
            try
            {
                List<DB.Manager.Account> accounts = DB.Manager.Account.Accounts;
                IEnumerable<string> lines = accounts.Select(x => $"\n账户:{x.UserId}\n权限:{x.Group.Name}");
                Show(lines.ToList());
            }
            catch (AccountException ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.MessageBuilder
                .Text("语法错误，正确的语法:\n")
                .Text($"{args.CommamdPrefix}account add <组> at\n")
                .Text($"{args.CommamdPrefix}account del <组> at\n")
                .Text($"{args.CommamdPrefix}account add <QQ> <组>\n")
                .Text($"{args.CommamdPrefix}account del <QQ> <组>\n")
                .Text($"{args.CommamdPrefix}account group <组> at\n")
                .Text($"{args.CommamdPrefix}account group <QQ> <组>\n")
                .Text($"{args.CommamdPrefix}account list")
                .Reply();
        }
    }
}
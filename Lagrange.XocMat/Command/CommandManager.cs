using System.Drawing;
using System.Reflection;
using System.Text;
using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Terraria.Protocol.PlayerMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command;

public class CommandManager(BotContext bot, ILogger<CommandManager> logger)
{
    public ILogger<CommandManager> Logger { get; } = logger;

    public BotContext Bot { get; } = bot;

    internal readonly List<Command> Commands = [];

    private void AddCommand(Command command)
    {
        Commands.Add(command);
    }

    private static List<string> ParseParameters(string str)
    {
        List<string> ret = [];
        var sb = new StringBuilder();
        bool instr = false;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            if (c == '\\' && ++i < str.Length)
            {
                if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
                    sb.Append('\\');
                sb.Append(str[i]);
            }
            else if (c == '"')
            {
                instr = !instr;
                if (!instr)
                {
                    ret.Add(sb.ToString());
                    sb.Clear();
                }
                else if (sb.Length > 0)
                {
                    ret.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else if (IsWhiteSpace(c) && !instr)
            {
                if (sb.Length > 0)
                {
                    ret.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
                sb.Append(c);
        }
        if (sb.Length > 0)
            ret.Add(sb.ToString());

        return ret;
    }

    private static bool IsWhiteSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n';
    }

    private static Dictionary<string, string> ParseCommandLine(List<string> command)
    {
        Dictionary<string, string> args = [];
        for (int i = 0; i < command.Count; i++)
        {
            string cmd = command[i];
            if (cmd.StartsWith('-'))
            {
                string str = "";
                for (int j = i + 1; j < command.Count; j++)
                {
                    if (!command[j].StartsWith('-'))
                        str += " " + command[j];
                    else
                        break;
                }
                if (!string.IsNullOrEmpty(str.Trim()))
                    args[cmd] = str.Trim();
            }
        }
        return args;
    }

    private CommandParis? CanRun(string text, uint uin)
    {
        if(string.IsNullOrEmpty(text) || uin == Bot.BotUin)
            return null;
        var prefix = XocMatSetting.Instance.CommamdPrefix.FirstOrDefault(text.StartsWith) ?? string.Empty;
        if (XocMatSetting.Instance.CommamdPrefix.Count > 0 && string.IsNullOrEmpty(prefix))
            return null;
        List<string> cmdParam = ParseParameters(text[prefix.Length..]);
        if (cmdParam.Count > 0)
        {
            var cmdName = cmdParam[0];
            cmdParam.RemoveAt(0);
            var account = Account.GetAccountNullDefault(uin);
            return Commands.Select(command => command.Alias.Contains(cmdName.ToLower()) switch
            {
                true => new CommandParis(command, cmdParam, cmdName, account, ParseCommandLine(cmdParam), prefix),
                false => null
            }).FirstOrDefault(x => x != null);
        }
        return null;
    }


    internal async void Adapter<T>(BotContext bot, T args)
    {
        CommandParis? comm;
        BaseCommandArgs commandArgs;
        switch (args)
        {
            case GroupMessageEvent groupMessageEvent:
                comm = CanRun(groupMessageEvent.Chain.GetText(), groupMessageEvent.Chain.GroupMemberInfo?.Uin ?? bot.BotUin);
                if(comm == null)
                    return;
                commandArgs = new GroupCommandArgs(bot, comm.Name, groupMessageEvent, comm.Prefix, comm.CmdParams, comm.CommandLine, comm.Account);
                break;
            case FriendMessageEvent friendMessageEvent:
                comm = CanRun(friendMessageEvent.Chain.GetText(), friendMessageEvent.Chain.FriendUin);
                if (comm == null)
                    return;
                commandArgs = new FriendCommandArgs(bot, comm.Name, friendMessageEvent, comm.Prefix, comm.CmdParams, comm.CommandLine, comm.Account);
                break;
            case PlayerCommandMessage playerCommandMessage:
                comm = CanRun(playerCommandMessage.Command, (uint)playerCommandMessage.Account.UserId);
                if (comm == null)
                    return;
                commandArgs = new ServerCommandArgs(XocMatAPI.BotContext, playerCommandMessage.Server!, playerCommandMessage.User!, playerCommandMessage.Account, comm.Name, comm.Prefix, comm.CmdParams, comm.CommandLine);
                break;
            default:
                throw new NotImplementedException();
        }
        if (comm.Command.Permissions.Any(comm.Account.HasPermission))
        {
            if (!await OperatHandler.Command(commandArgs))
            {
                try
                {
                    var log = XocMatApp.Instance.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(comm.Command.GetType())) as ILogger ?? Logger;
                    await comm.Command.InvokeAsync(commandArgs, log);
                    Logger.LogInformation("{Logger}", commandArgs.ToPreviewString());
                }
                catch (Exception e)
                {
                    await ReplyMessage("命令执行失败，请查看日志", commandArgs
                        );
                    Logger.LogInformation("{Error}", commandArgs.ToPerviewErrorString(e));
                }
            }
            return;
        }
        Logger.LogInformation("{Skipping}", commandArgs.ToSkippingString());
        await ReplyMessage("你无权使用此命令！", commandArgs);
    }

    private static Task ReplyMessage(string msg, BaseCommandArgs args)
    {
        return args switch
        {
            GroupCommandArgs groupCommandArgs => groupCommandArgs.Event.Reply(msg),
            FriendCommandArgs friendCommandArgs => friendCommandArgs.Event.Reply(msg),
            ServerCommandArgs serverCommandArgs => serverCommandArgs.Reply(msg, Color.DarkRed),
            _ => throw new NotImplementedException(),
        };
    }

    internal List<Command> RegisterCommand(Assembly assembly) => [.. assembly.GetExportedTypes()
        .Where(type => type.IsSubclassOf(typeof(Command)))
        .Select(type => Activator.CreateInstance(type) as Command)
        .Where(instance => instance != null)
        .Select(instance =>
        {
            AddCommand(instance!);
            return instance;
        })];
}

public record CommandParis(Command Command, List<string> CmdParams, string Name, Account Account, Dictionary<string, string> CommandLine, string Prefix);

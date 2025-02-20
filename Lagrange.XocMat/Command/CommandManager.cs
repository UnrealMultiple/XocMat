using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace Lagrange.XocMat.Command;

public class CommandManager
{
    public ILogger<CommandManager> Logger { get; }

    public BotContext Bot { get; }

    public readonly List<Command> Commands = [];

    public CommandManager(BotContext bot, ILogger<CommandManager> logger)
    {
        Bot = bot;
        Logger = logger;
        Bot.Invoker.OnGroupMessageReceived += async (bot, e) => await GroupCommandAdapter(bot, e);
    }

    private void AddCommand(Command command)
    {
        Commands.Add(command);
    }

    public static List<string> ParseParameters(string str)
    {
        var ret = new List<string>();
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
        var args = new Dictionary<string, string>();
        for (int i = 0; i < command.Count; i++)
        {
            var cmd = command[i];
            if (cmd.StartsWith("-"))
            {
                var str = "";
                for (int j = i + 1; j < command.Count; j++)
                {
                    if (!command[j].StartsWith("-"))
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
    private RunCommandParams? Run(string text, uint uin)
    {
        string prefix = string.Empty;
        foreach (var x in XocMatSetting.Instance.CommamdPrefix)
        {
            if (text.StartsWith(x))
            {
                prefix = x;
                break;
            }
        }
        var cmdParam = ParseParameters(text[prefix.Length..]);
        if (cmdParam.Count > 0)
        {
            var cmdName = cmdParam[0];
            cmdParam.RemoveAt(0);
            var account = Account.GetAccountNullDefault(uin);
            foreach (var command in Commands.ToArray())
            {
                if (command.Name.Contains(cmdName.ToLower()))
                {
                    return new RunCommandParams(command, cmdParam, cmdName, account, ParseCommandLine(cmdParam), prefix);
                }
            }
        }
        return null;
    }


    public async ValueTask GroupCommandAdapter(BotContext bot, GroupMessageEvent args)
    {
        var comm = Run(args.Chain.GetText(), args.Chain.GroupMemberInfo!.Uin);
        if(comm == null)
            return;
        var commandArgs = new GroupCommandArgs(bot, args.Chain.GroupMemberInfo!.MemberName, args, comm.Prefix, comm.CmdParams, comm.CommandLine, comm.Account);
        if (comm.Account.HasPermission(comm.Command.Permission))
        {
            if (!await OperatHandler.UserCommand(commandArgs))
            {
                await comm.Command.InvokeAsync(commandArgs);
                Logger.LogInformation($"group:{args.Chain.GroupUin} {args.Chain.GroupMemberInfo!.MemberName}({args.Chain.GroupMemberInfo!.Uin}) 使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Cyan);
            }
            return;
        }
        Logger.LogInformation($"group: {args.Chain.GroupUin} {args.Chain.GroupMemberInfo!.MemberName}({args.Chain.GroupMemberInfo.Uin}) 试图使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Yellow);
        await args.Reply("你无权使用此命令！");
    }

    internal async Task CommandAdapter(PlayerCommandMessage args)
    {
        var server = XocMatSetting.Instance.GetServer(args.ServerName);
        var user = TerrariaUser.GetUsersByName(args.Name, args.ServerName);
        var account = Account.GetAccountNullDefault(user == null ? 0 : user.Id);
        if (account == null || server == null || user == null)
            return;
        var comm = Run(args.Command, (uint)account.UserId);
        if (comm == null)
            return;
        var commandArgs = new ServerCommandArgs(XocMatAPI.BotContext, args.ServerName, args.Name, comm.Name, comm.Prefix, comm.CmdParams, comm.CommandLine);

        if (comm.Account.HasPermission(comm.Command.Permission))
        {
            if (!await OperatHandler. ServerUserCommand(commandArgs))
            {
                await comm.Command.InvokeAsync(commandArgs);
                Logger.LogInformation($"server:{user.Name} ({user.Id}) 使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Cyan);
            }
            return;
        }
        Logger.LogInformation($"server: {user.Name}  ( {user.Id}) 试图使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Yellow);
        await server.PrivateMsg(args.Name, "你无权使用此命令！", Color.DarkRed);
    }


    public List<Command> RegisterCommand(Assembly assembly)
    {
        var cmds = new List<Command>();
        foreach (var type in assembly.GetExportedTypes())
        {
             if(type.IsSubclassOf(typeof(Command)))
            {
                if (Activator.CreateInstance(type) is not Command instance)
                    continue;
                cmds.Add(instance);
                AddCommand(instance);
            }
        }
        return cmds;
    }
}

public record RunCommandParams(Command Command, List<string> CmdParams, string Name, Account Account, Dictionary<string, string> CommandLine,  string Prefix);

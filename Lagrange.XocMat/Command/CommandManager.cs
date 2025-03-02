using System.Drawing;
using System.Reflection;
using System.Text;
using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Command;

public class CommandManager
{
    public ILogger<CommandManager> Logger { get; }

    public BotContext Bot { get; }

    internal readonly List<Command> Commands = [];

    public CommandManager(BotContext bot, ILogger<CommandManager> logger)
    {
        Bot = bot;
        Logger = logger;
        Bot.Invoker.OnGroupMessageReceived += async (bot, e) => await GroupCommandAdapter(bot, e);
        Bot.Invoker.OnFriendMessageReceived += async (bot, e) => await FriendCommandAdapter(bot, e);

    }

    private void AddCommand(Command command)
    {
        Commands.Add(command);
    }

    public static List<string> ParseParameters(string str)
    {
        List<string> ret = [];
        StringBuilder sb = new StringBuilder();
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
            if (cmd.StartsWith("-"))
            {
                string str = "";
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
        if(uin == Bot.BotUin)
            return null;
        string? prefix = null;
        if(XocMatSetting.Instance.CommamdPrefix.Count == 0)
            prefix = "";
        else
            prefix = XocMatSetting.Instance.CommamdPrefix.FirstOrDefault(text.StartsWith);
        if(prefix == null)
            return null;
        List<string> cmdParam = ParseParameters(text[prefix.Length..]);
        if (cmdParam.Count > 0)
        {
            string cmdName = cmdParam[0];
            cmdParam.RemoveAt(0);
            Account account = Account.GetAccountNullDefault(uin);
            foreach (Command command in Commands.ToArray())
            {
                if (command.Alias.Contains(cmdName.ToLower()))
                {
                    return new RunCommandParams(command, cmdParam, cmdName, account, ParseCommandLine(cmdParam), prefix);
                }
            }
        }
        return null;
    }

    internal async ValueTask GroupCommandAdapter(BotContext bot, GroupMessageEvent args)
    {
        RunCommandParams? comm = Run(args.Chain.GetText(), args.Chain.GroupMemberInfo!.Uin);
        if (comm == null)
            return;
        GroupCommandArgs commandArgs = new(bot, comm.Name, args, comm.Prefix, comm.CmdParams, comm.CommandLine, comm.Account);
        if (comm.Command.Permissions.Length == 0 || comm.Command.Permissions.Any(comm.Account.HasPermission))
        {
            if (!await OperatHandler.GroupCommand(commandArgs))
            {
                try
                {
                    var log = XocMatApp.Instance.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(comm.Command.GetType())) as ILogger ?? Logger;
                    await comm.Command.InvokeAsync(commandArgs, log);
                    Logger.LogInformation($"Group Command:{args.Chain.GroupUin} {args.Chain.GroupMemberInfo!.MemberName}({args.Chain.GroupMemberInfo!.Uin}) 使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Cyan);
                }
                catch (Exception e)
                {
                    await args.Reply("命令执行失败，请查看日志",true); 
                    Logger.LogError(e, $"Group Command:{args.Chain.GroupUin} {args.Chain.GroupMemberInfo!.MemberName}({args.Chain.GroupMemberInfo!.Uin}) 使用命令: {comm.Prefix}{comm.Name} 时发生错误");
                }
            }
            return;
        }
        Logger.LogInformation($"Group Command: {args.Chain.GroupUin} {args.Chain.GroupMemberInfo!.MemberName}({args.Chain.GroupMemberInfo.Uin}) 试图使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Yellow);
        await args.Reply("你无权使用此命令！");
    }

    private async Task FriendCommandAdapter(BotContext bot, FriendMessageEvent args)
    {
        RunCommandParams? comm = Run(args.Chain.GetText(), args.Chain.FriendUin);
        if (comm == null)
            return;
        FriendCommandArgs commandArgs = new FriendCommandArgs(bot, comm.Name, args, comm.Prefix, comm.CmdParams, comm.CommandLine, comm.Account);
        if (comm.Command.Permissions.Any(comm.Account.HasPermission))
        {
            if (!await OperatHandler.FriendCommand(commandArgs))
            {
                try
                {
                    var log = XocMatApp.Instance.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(comm.Command.GetType())) as ILogger ?? Logger;
                    await comm.Command.InvokeAsync(commandArgs, log);
                    Logger.LogInformation($"Friend Command: {args.Chain.FriendInfo!.Nickname}({args.Chain.FriendUin}) 使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Cyan);
                }
                catch(Exception e)
                {
                    await args.Reply("命令执行失败，请查看日志", true);
                    Logger.LogError(e, $"Friend Command: {args.Chain.FriendInfo!.Nickname}({args.Chain.FriendUin}) 使用命令: {comm.Prefix}{comm.Name} 时发生错误");
                }
            }
            return;
        }
        Logger.LogInformation($"Friend Command: {args.Chain.FriendInfo!.Nickname}({args.Chain.FriendUin}) 试图使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Yellow);
        await args.Reply("你无权使用此命令！");
    }


    internal async Task CommandAdapter(PlayerCommandMessage args)
    {
        Terraria.TerrariaServer? server = XocMatSetting.Instance.GetServer(args.ServerName);
        TerrariaUser? user = TerrariaUser.GetUsersByName(args.Name, args.ServerName);
        Account account = Account.GetAccountNullDefault(user == null ? 0 : user.Id);
        if (account == null || server == null || user == null)
            return;
        RunCommandParams? comm = Run(args.Command, (uint)account.UserId);
        if (comm == null)
            return;
        ServerCommandArgs commandArgs = new ServerCommandArgs(XocMatAPI.BotContext, server, user, account, comm.Name, comm.Prefix, comm.CmdParams, comm.CommandLine);

        if (comm.Command.Permissions.Any(comm.Account.HasPermission))
        {
            if (!await OperatHandler.ServerUserCommand(commandArgs))
            {
                try
                {
                    var log = XocMatApp.Instance.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(comm.Command.GetType())) as ILogger ?? Logger;
                    await comm.Command.InvokeAsync(commandArgs, log);
                    Logger.LogInformation($"Server Command:{user.Name} ({user.Id}) 使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Cyan);
                }
                catch(Exception e)
                {
                    await server.PrivateMsg(args.Name, "命令执行失败，请查看日志", Color.GreenYellow);
                    Logger.LogError(e, $"Server Command:{user.Name} ({user.Id}) 使用命令: {comm.Prefix}{comm.Name} 时发生错误");
                }
            }
            return;
        }
        Logger.LogInformation($"Server Command: {user.Name}  ( {user.Id}) 试图使用命令: {comm.Prefix}{comm.Name}", ConsoleColor.Yellow);
        await server.PrivateMsg(args.Name, "你无权使用此命令！", Color.DarkRed);
    }


    internal List<Command> RegisterCommand(Assembly assembly)
    {
        List<Command> cmds = [];
        foreach (Type type in assembly.GetExportedTypes())
        {
            if (type.IsSubclassOf(typeof(Command)))
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

public record RunCommandParams(Command Command, List<string> CmdParams, string Name, Account Account, Dictionary<string, string> CommandLine, string Prefix);

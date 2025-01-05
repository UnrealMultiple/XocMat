using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace Lagrange.XocMat.Commands;

public class CommandManager
{
    public ILogger<CommandManager> Logger { get; }

    public BotContext Bot { get; }

    public readonly List<Command<CommandArgs>> GroupCommandDelegate = [];

    public readonly List<Command<ServerCommandArgs>> ServerCommandDelegate = [];

    public CommandManager(BotContext bot, ILogger<CommandManager> logger)
    {
        Bot = bot;
        Logger = logger;
        Bot.Invoker.OnGroupMessageReceived += async (BotContext bot, GroupMessageEvent e) => await CommandAdapter(bot, e);
    }


    public void AddGroupCommand(Command<CommandArgs> command)
    {
        GroupCommandDelegate.Add(command);
    }

    public void AddServerCommand(Command<ServerCommandArgs> command)
    {
        ServerCommandDelegate.Add(command);
    }

    public List<string> ParseParameters(string str)
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

    private bool IsWhiteSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n';
    }

    private Dictionary<string, string> ParseCommandLine(List<string> command)
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

    public async ValueTask CommandAdapter(BotContext bot, GroupMessageEvent args)
    {
        var text = args.Chain.GetText();
        string prefix = string.Empty;
        XocMatSetting.Instance.CommamdPrefix.ForEach(x =>
        {
            if (text.StartsWith(x))
            {
                prefix = x;
            }
        });
        if (!string.IsNullOrEmpty(prefix))
        {
            var cmdParam = ParseParameters(text[prefix.Length..]);
            if (cmdParam.Count > 0)
            {
                var cmdName = cmdParam[0];
                cmdParam.RemoveAt(0);
                var account = Account.GetAccountNullDefault(args.Chain.GroupMemberInfo!.Uin);
                foreach(var command in GroupCommandDelegate.ToArray())
                { 
                    if (command.Name.Contains(cmdName))
                    {
                        try
                        {
                            await RunCommandCallback(new CommandArgs(bot, cmdName, args, prefix, cmdParam, ParseCommandLine(cmdParam), account), command);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex.ToString());
                            await args.Reply("使用此命令时发生错误，错误详情请查看日志!", true);
                        }
                    }
                }
            }
        }
    }

    private async ValueTask RunCommandCallback(CommandArgs args, Command<CommandArgs> command)
    {
        foreach (var perm in command.Permission)
        {
            if (args.Account.HasPermission(perm))
            {
                if (!await OperatHandler.UserCommand(args))
                {
                    await command.CallBack(args);
                    Logger.LogInformation($"group:{args.EventArgs.Chain.GroupUin} {args.EventArgs.Chain.GroupMemberInfo!.MemberName}({args.EventArgs.Chain.GroupMemberInfo!.Uin}) 使用命令: {args.CommamdPrefix}{args.Name}", ConsoleColor.Cyan);
                }
                return;
            }
        }
        Logger.LogInformation($"group: {args.EventArgs.Chain.GroupUin} {args.EventArgs.Chain.GroupMemberInfo!.MemberName}({args.EventArgs.Chain.GroupMemberInfo.Uin}) 试图使用命令: {args.CommamdPrefix}{args.Name}", ConsoleColor.Yellow);
        await args.EventArgs.Reply("你无权使用此命令！");
    }

    public async ValueTask CommandAdapter(PlayerCommandMessage args)
    {
        var text = args.Command;
        var cmdParam = ParseParameters(text[args.CommandPrefix.Length..]);
        if (cmdParam.Count > 0)
        {
            var cmdName = cmdParam[0];
            cmdParam.RemoveAt(0);
            foreach (var command in ServerCommandDelegate)
            {
                if (command.Name.Contains(cmdName))
                {
                    try
                    {
                        await RunCommandCallback(new ServerCommandArgs(Bot, args.ServerName, args.Name, cmdName, args.CommandPrefix, cmdParam, ParseCommandLine(cmdParam)), command);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.ToString());
                        if (args.TerrariaServer != null)
                            await args.TerrariaServer.PrivateMsg(args.Name, ex.Message, Color.DarkRed);
                    }
                }
            }
        }
    }

    private async ValueTask RunCommandCallback(ServerCommandArgs args, Command<ServerCommandArgs> command)
    {
        foreach (var perm in command.Permission)
        {
            if (args.Account.HasPermission(perm))
            {
                if (!await OperatHandler.ServerUserCommand(args))
                {
                    await command.CallBack(args);
                    Logger.LogInformation($"Server:{args.ServerName} {args.UserName} 使用命令: {command.Name.First()}", ConsoleColor.Cyan);
                }
                return;
            }
        }
    }

    public void RegisterCommand(Type type)
    {
        var flag = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
        var methods = type.GetMethods(flag)
            .Where(m => m.IsDefined(typeof(CommandMatch)) && (m.CommandParamPares(typeof(CommandArgs)) || m.CommandParamPares(typeof(ServerCommandArgs))))
            .ToArray();
        var instance = Activator.CreateInstance(type);
        if (instance == null)
            return;
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<CommandMatch>()!;
            if (method.IsStatic)
            {
                if (method.CommandParamPares(typeof(CommandArgs)))
                    AddGroupCommand(new(attr.Name, method.CreateDelegate<Command<CommandArgs>.CommandCallBack>(), attr.Permission));
                else
                    AddServerCommand(new(attr.Name, method.CreateDelegate<Command<ServerCommandArgs>.CommandCallBack>(), attr.Permission));
                continue;
            }
            var _method = instance.GetType().GetMethod(method.Name, flag)!;
            if (method.CommandParamPares(typeof(CommandArgs)))
                AddGroupCommand(new(attr.Name, _method.CreateDelegate<Command<CommandArgs>.CommandCallBack>(instance), attr.Permission));
            else
                AddServerCommand(new(attr.Name, _method.CreateDelegate<Command<ServerCommandArgs>.CommandCallBack>(instance), attr.Permission));
        }
    }

    public void RegisterCommand(Assembly assembly)
    {
        var mapping = assembly.GetExportedTypes()
            .Where(x => x.IsDefined(typeof(CommandSeries)));
        foreach (var type in mapping)
        {
            RegisterCommand(type);
        }
    }
}

using Lagrange.Core;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.EventArgs.Sockets;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Terraria.Protocol;
using Lagrange.XocMat.Terraria.Protocol.Action;
using Lagrange.XocMat.Terraria.Protocol.Action.Receive;
using Lagrange.XocMat.Terraria.Protocol.Action.Response;
using Lagrange.XocMat.Terraria.Protocol.PlayerMessage;
using Lagrange.XocMat.Terraria.Protocol.ServerMessage;
using Lagrange.XocMat.Utility;
using LinqToDB;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace Lagrange.XocMat.Event;

public class SocketAdapter
{
    public delegate TResult EventCallBack<in TEventArgs, out TResult>(TEventArgs args);

    public static event EventCallBack<PlayerJoinMessage, ValueTask>? OnPlayerJoin;

    public static event EventCallBack<PlayerLeaveMessage, ValueTask>? OnPlayerLeave;

    public static event EventCallBack<PlayerChatMessage, ValueTask>? OnPlayerChat;

    public static event EventCallBack<PlayerCommandMessage, ValueTask>? OnPlayerCommand;

    public static event EventCallBack<GameInitMessage, ValueTask>? OnGameInit;

    public static event EventCallBack<BaseMessage, ValueTask>? OnConnect;

    public static event EventCallBack<BaseMessage, ValueTask>? OnHeartBeat;

    private readonly ReplaySubject<BaseAction> ApiSubject = new(2);

    private readonly Dictionary<PostMessageType, EventCallBack<ServerMsgArgs, ValueTask>> _action;

    public CommandManager CommandManager { get; }

    public ILogger<SocketAdapter> Logger { get; }

    public WebSocketServer WebSocketServer { get; }

    public SocketAdapter(BotContext bot, CommandManager cmdManager, WebSocketServer wsServer, ILogger<SocketAdapter> logger)
    {
        CommandManager = cmdManager;
        Logger = logger;
        WebSocketServer = wsServer;
        _action = new()
        {
            { PostMessageType.Action, ActionHandler },
            { PostMessageType.PlayerJoin, PlayerJoinHandler },
            { PostMessageType.PlayerLeave, PlayerLeaveHandler },
            { PostMessageType.PlayerCommand, PlayerCommandHandler },
            { PostMessageType.PlayerMessage, PlayerMessageHandler },
            { PostMessageType.GamePostInit, GamePostInitHandler },
            { PostMessageType.Connect, ConnectHandler },
            { PostMessageType.HeartBeat, HeartBeatHandler },
        };
    }


    private async ValueTask PlayerMessageHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is not PlayerChatMessage data) return;
        if (OnPlayerChat != null) await OnPlayerChat(data);
        if (data.Handler || data.TerrariaServer == null || data.Mute || data.Text.Length >= data.TerrariaServer.MsgMaxLength) return;
        var tasks = data.TerrariaServer.ForwardGroups.Select(uin => MessageBuilder.Group(uin).Text($"[{data.TerrariaServer.Name}] {data.Name}: {data.Text}").Reply());
        await Task.WhenAll(tasks);
    }

    private async ValueTask HeartBeatHandler(ServerMsgArgs args)
    {
        if (OnHeartBeat != null) await OnHeartBeat(args.BaseMessage);
        await ValueTask.CompletedTask;
    }

    private async ValueTask ConnectHandler(ServerMsgArgs args)
    {
        if (OnConnect != null) await OnConnect(args.BaseMessage);
        Logger.LogInformation("[{Time}] [Terraria Server Connect]: {ServerName} 已连接...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), args.BaseMessage.ServerName);
        await args.BaseMessage.TerrariaServer!.ReplyConnectStatus();
    }

    private async ValueTask GamePostInitHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is not GameInitMessage data) return;
        if (OnGameInit != null) await OnGameInit(data);
        if (data.Handler || data.TerrariaServer == null) return;
        var tasks = data.TerrariaServer.ForwardGroups.Select(uin => MessageBuilder.Group(uin).Text($"[{data.TerrariaServer.Name}]服务器初始化已完成..").Reply());
        await Task.WhenAll(tasks);
    }

    private async ValueTask PlayerCommandHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is not PlayerCommandMessage data) return;
        if (OnPlayerCommand != null) await OnPlayerCommand(data);
        if (!data.Handler && data.HasServerUser) CommandManager.Adapter(XocMatAPI.BotContext, data);
    }

    private async ValueTask PlayerLeaveHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is not PlayerLeaveMessage data) return;
        if (OnPlayerLeave != null) await OnPlayerLeave(data);
        if (data.Handler || data.TerrariaServer == null) return;
        var tasks = data.TerrariaServer.ForwardGroups.Select(uin => MessageBuilder.Group(uin).Text($"[{data.TerrariaServer.Name}] {data.Name}离开服务器..").Reply());
        await Task.WhenAll(tasks);
    }

    private async ValueTask PlayerJoinHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is not PlayerJoinMessage data) return;
        if (OnPlayerJoin != null) await OnPlayerJoin(data);
        if (data.Handler || data.TerrariaServer == null) return;
        var tasks = data.TerrariaServer.ForwardGroups.Select(uin => MessageBuilder.Group(uin).Text($"[{data.TerrariaServer.Name}] {data.Name}进入服务器..").Reply());
        await Task.WhenAll(tasks);
    }

    private ValueTask ActionHandler(ServerMsgArgs args)
    {
        if (args.BaseMessage is BaseAction msg) ApiSubject.OnNext(msg);
        return ValueTask.CompletedTask;
    }

    internal async ValueTask<T?> GetResponse<T>(string echo, TimeSpan? timeout = null) where T : BaseActionResponse, new()
    {
        try
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(15);
            var task = ApiSubject.Where(x => x.Echo == echo)
                .Select(x => x as T)
                .Timeout((TimeSpan)timeout)
                .Take(1)
                .ToTask();
            var action = await task;
            return action;
        }
        catch (Exception ex)
        {
            return new T()
            {
                Status = false,
                Message = $"与服务器通信发生错误:{ex.Message}"
            };
        }
    }

    public async Task Adapter(string identity, byte[] payload)
    {
        try
        {
            var baseMessage = Serializer.Deserialize<BaseMessage>(new ReadOnlyMemory<byte>(payload));
            if (baseMessage.TerrariaServer != null
                && baseMessage.Token == baseMessage.TerrariaServer.Token
                && _action.TryGetValue(baseMessage.MessageType, out EventCallBack<ServerMsgArgs, ValueTask>? action))
            {
                await action(new(baseMessage, identity));
            }
            else
            {
                string echo = Guid.NewGuid().ToString();
                using var ms = new MemoryStream();
                var response = new SocketConnectStatusArgs()
                {
                    ServerName = baseMessage.ServerName,
                    ActionType = ActionType.ConnectStatus,
                    Token = baseMessage.Token,
                    Echo = echo,
                };
                if (baseMessage.TerrariaServer == null)
                {
                    Logger.LogError("[{Time}] [Terraria Server Connect Error]: 接受到{ServerName} 的连接请求但，在配置文件中没有找到{ServerName}服务器!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), baseMessage.ServerName, baseMessage.ServerName);
                    response.Status = SocketConnentType.ServerNull;

                }
                else if (baseMessage.Token != baseMessage.TerrariaServer?.Token)
                {
                    response.Status = SocketConnentType.VerifyError;
                    Logger.LogError("[{Time}] [Terraria Server Connect Error]: {ServerName} 的Token 与配置文件不匹配!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), baseMessage.ServerName);
                }
                else
                {
                    Logger.LogError("[{Time}] [Terraria Server Connect Error]: {ServerName} 未知连接错误!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), baseMessage.ServerName);
                    response.Status = SocketConnentType.Error;
                }
                Serializer.Serialize(ms, response);
                await WebSocketServer.SendBytesAsync(ms.ToArray(), identity);
                await WebSocketServer.DisconnectAsync(identity, System.Net.WebSockets.WebSocketCloseStatus.NormalClosure);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("[{Time}] [Protocol Deserialize Error]: 协议解析错误 {Error}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex);
        }

    }

    internal static async void GroupMessageForwardAdapter(BotContext bot, GroupMessageEvent args)
    {
        if (args.Chain.GroupMemberInfo?.Uin == bot.BotUin)
        {
            return;
        }
        var file = args.Chain.GetFile();
        if (file != null && file.FileSize < 1024 * 1024 * 30)
        {
            foreach (var setting in XocMatSetting.Instance.Servers)
            {
                if (file.FileUrl != null && setting != null && setting.Groups.Contains(args.Chain.GroupUin ?? 0) && setting.WaitFile != null)
                {
                    setting.WaitFile.SetResult(await HttpUtils.GetByteAsync(file.FileUrl));
                }
            }
        }
        string text = args.Chain.GetText();
        if (string.IsNullOrEmpty(text))
            return;
        var eventArgs = new GroupMessageForwardArgs()
        {
            Handler = false,
            GroupMessageEventArgs = args,
            Context = text,
        };
        if (!await OperatHandler.MessageForward(eventArgs))
        {
            var tasks = XocMatSetting.Instance.Servers
                .Where(s => s is not null && s.MsgMaxLength > text.Length && s.ForwardGroups.Contains(args.Chain.GroupUin!.Value))
                .Select(s => s.Broadcast($"[群消息][{args.Chain.GroupMemberInfo?.MemberCard}]: {text}", System.Drawing.Color.GreenYellow));
            await Task.WhenAll(tasks);
        }

    }
}

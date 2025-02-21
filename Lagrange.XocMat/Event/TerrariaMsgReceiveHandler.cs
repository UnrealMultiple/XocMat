using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.EventArgs.Sockets;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal.Socket;
using Lagrange.XocMat.Internal.Socket.Action;
using Lagrange.XocMat.Internal.Socket.Action.Receive;
using Lagrange.XocMat.Internal.Socket.Action.Response;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Lagrange.XocMat.Internal.Socket.ServerMessage;
using Lagrange.XocMat.Net;
using Lagrange.XocMat.Utility;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace Lagrange.XocMat.Event;

public class TerrariaMsgReceiveHandler
{
    public delegate TResult EventCallBack<in TEventArgs, out TResult>(TEventArgs args);

    public static event EventCallBack<PlayerJoinMessage, ValueTask>? OnPlayerJoin;

    public static event EventCallBack<PlayerLeaveMessage, ValueTask>? OnPlayerLeave;

    public static event EventCallBack<PlayerChatMessage, ValueTask>? OnPlayerChat;

    public static event EventCallBack<PlayerCommandMessage, ValueTask>? OnPlayerCommand;

    public static event EventCallBack<GameInitMessage, ValueTask>? OnGameInit;

    public static event EventCallBack<BaseMessage, ValueTask>? OnConnect;

    public static event EventCallBack<BaseMessage, ValueTask>? OnHeartBeat;

    private readonly ReplaySubject<(BaseAction, byte[])> ApiSubject = new(2);

    private readonly Dictionary<PostMessageType, EventCallBack<ServerMsgArgs, ValueTask>> _action;

    public BotContext Bot { get; }

    public CommandManager CommandManager { get; }

    public TShockReceive Receive { get; }

    public ILogger<TerrariaMsgReceiveHandler> Logger { get; }

    public TerrariaMsgReceiveHandler(BotContext bot, CommandManager cmdManager, TShockReceive receive, ILogger<TerrariaMsgReceiveHandler> logger)
    {
        Bot = bot;
        CommandManager = cmdManager;
        Receive = receive;
        Logger = logger;
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
        Receive.SocketMessage += Adapter;
        Bot.Invoker.OnGroupMessageReceived += GroupMessageForwardAdapter;
    }

    private async ValueTask PlayerMessageHandler(ServerMsgArgs args)
    {
        PlayerChatMessage data = Serializer.Deserialize<PlayerChatMessage>(args.Stream);
        if (OnPlayerChat != null)
            await OnPlayerChat(data);
        if (!data.Handler && data.TerrariaServer != null && !data.Mute)
        {
            if (data.Text.Length >= data.TerrariaServer.MsgMaxLength)
                return;
            foreach (uint group in data.TerrariaServer.ForwardGroups)
            {
                await Bot.SendMessage(MessageBuilder.Group(group).Text($"[{data.TerrariaServer.Name}] {data.Name}: {data.Text}").Build());
            }
        }
    }

    private async ValueTask HeartBeatHandler(ServerMsgArgs args)
    {
        BaseMessage data = Serializer.Deserialize<BaseMessage>(args.Stream);
        WebSocketConnectManager.Add(data.ServerName, args.id);
        if (OnHeartBeat != null)
            await OnHeartBeat(data);
        await ValueTask.CompletedTask;
    }

    private async ValueTask ConnectHandler(ServerMsgArgs args)
    {
        BaseMessage data = Serializer.Deserialize<BaseMessage>(args.Stream);
        WebSocketConnectManager.Add(data.ServerName, args.id);
        if (OnConnect != null) await OnConnect(data);
        Logger.LogInformation($"Terraria Server {data.ServerName} {args.id} 已连接...", ConsoleColor.Green);
        await data.TerrariaServer!.ReplyConnectStatus();
    }

    private async ValueTask GamePostInitHandler(ServerMsgArgs args)
    {
        GameInitMessage data = Serializer.Deserialize<GameInitMessage>(args.Stream);
        if (OnGameInit != null) await OnGameInit(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (uint group in data.TerrariaServer.ForwardGroups)
            {
                await Bot.SendMessage(MessageBuilder.Group(group).Text($"[{data.TerrariaServer.Name}]服务器初始化已完成..").Build());
            }
        }
    }

    private async ValueTask PlayerCommandHandler(ServerMsgArgs args)
    {
        PlayerCommandMessage data = Serializer.Deserialize<PlayerCommandMessage>(args.Stream);
        if (OnPlayerCommand != null) await OnPlayerCommand(data);
        if (!data.Handler)
        {
            await CommandManager.CommandAdapter(data);
        }
    }

    private async ValueTask PlayerLeaveHandler(ServerMsgArgs args)
    {
        PlayerLeaveMessage data = Serializer.Deserialize<PlayerLeaveMessage>(args.Stream);
        if (OnPlayerLeave != null) await OnPlayerLeave(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (uint group in data.TerrariaServer.ForwardGroups)
            {
                await Bot.SendMessage(MessageBuilder.Group(group).Text($"[{data.TerrariaServer.Name}] {data.Name}离开服务器..").Build());
            }
        }
    }

    private async ValueTask PlayerJoinHandler(ServerMsgArgs args)
    {
        PlayerJoinMessage data = Serializer.Deserialize<PlayerJoinMessage>(args.Stream);
        if (OnPlayerJoin != null) await OnPlayerJoin(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (uint group in data.TerrariaServer.ForwardGroups)
            {
                await Bot.SendMessage(MessageBuilder.Group(group).Text($"[{data.TerrariaServer.Name}] {data.Name}进入服务器..").Build());
            }
        }
    }

    private ValueTask ActionHandler(ServerMsgArgs args)
    {
        BaseAction msg = Serializer.Deserialize<BaseAction>(args.Stream);
        ApiSubject.OnNext((msg, args.Stream.ToArray()));
        return ValueTask.CompletedTask;
    }

    internal async ValueTask<T?> GetResponse<T>(string echo, TimeSpan? timeout = null) where T : BaseActionResponse, new()
    {
        try
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(15);
            Task<byte[]> task = ApiSubject.Where(x => x.Item1.Echo == echo)
            .Select(x => x.Item2)
                .Timeout((TimeSpan)timeout)
                .Take(1)
                .ToTask();
            byte[] buffer = await task;
            using MemoryStream Stream = new MemoryStream(buffer);
            return Serializer.Deserialize<T>(Stream);
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

    public async ValueTask Adapter(SocketReceiveMessageArgs args)
    {
        try
        {
            BaseMessage baseMsg = Serializer.Deserialize<BaseMessage>(args.Stream);
            if (baseMsg.TerrariaServer != null
                && baseMsg.Token == baseMsg.TerrariaServer.Token
                && _action.TryGetValue(baseMsg.MessageType, out EventCallBack<ServerMsgArgs, ValueTask>? action))
            {
                args.Stream.Position = 0;
                await action(new()
                {
                    id = args.ConnectId,
                    Stream = args.Stream,
                    BaseMessage = baseMsg
                });
                args.Stream.Dispose();
            }
            else
            {
                string echo = Guid.NewGuid().ToString();
                using MemoryStream ms = new MemoryStream();
                SocketConnectStatusArgs response = new SocketConnectStatusArgs()
                {
                    ServerName = baseMsg.ServerName,
                    ActionType = ActionType.ConnectStatus,
                    Token = baseMsg.Token,
                    Echo = echo,
                };
                if (baseMsg.TerrariaServer == null)
                {
                    Logger.LogError($"接受到{baseMsg.ServerName} 的连接请求但，在配置文件中没有找到{baseMsg.ServerName}服务器!");
                    response.Status = SocketConnentType.ServerNull;

                }
                else if (baseMsg.Token != baseMsg.TerrariaServer?.Token)
                {
                    response.Status = SocketConnentType.VerifyError;
                    Logger.LogError($"{baseMsg.ServerName} 的Token 与配置文件不匹配!");
                }
                else
                {
                    Logger.LogError($"{baseMsg.ServerName} 未知连接错误!");
                    response.Status = SocketConnentType.Error;
                }
                Serializer.Serialize(ms, response);
                await Receive.Send(ms.ToArray(), args.ConnectId);
                await Receive.Close(args.ConnectId, System.Net.WebSockets.WebSocketCloseStatus.NormalClosure);
                ms.Dispose();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"解析信息是出现错误:{ex.Message}");
        }

    }

    internal async void GroupMessageForwardAdapter(BotContext bot, GroupMessageEvent args)
    {
        if (args.Chain.GroupMemberInfo!.Uin == bot.BotUin)
        {
            return;
        }
        Core.Message.Entity.FileEntity? file = args.Chain.GetFile();
        if (file != null && file.FileSize < 1024 * 1024 * 30)
        {
            foreach (Terraria.TerrariaServer setting in XocMatSetting.Instance.Servers)
            {
                if (file.FileUrl != null && setting != null && setting.Groups.Contains(args.Chain.GroupUin!.Value) && setting.WaitFile != null)
                {
                    setting.WaitFile.SetResult(await HttpUtils.HttpGetByte(file.FileUrl));
                }
            }
        }
        string text = args.Chain.GetText();
        if (string.IsNullOrEmpty(text))
            return;
        GroupMessageForwardArgs eventArgs = new GroupMessageForwardArgs()
        {
            Handler = false,
            GroupMessageEventArgs = args,
            Context = text,
        };
        if (!await OperatHandler.MessageForward(eventArgs))
        {
            foreach (Terraria.TerrariaServer server in XocMatSetting.Instance.Servers)
            {
                if (server != null && text.Length <= server.MsgMaxLength)
                {
                    if (server.ForwardGroups.Contains(Convert.ToUInt32(args.Chain.GroupUin)))
                    {
                        await server.Broadcast($"[群消息][{args.Chain.GroupMemberInfo?.MemberCard}]: {text}", System.Drawing.Color.GreenYellow);
                    }
                }
            }
        }

    }
}

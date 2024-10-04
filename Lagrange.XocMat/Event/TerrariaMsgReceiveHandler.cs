using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Commands;
using Lagrange.XocMat.Enumerates;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.EventArgs.Sockets;
using Lagrange.XocMat.Internal.Socket;
using Lagrange.XocMat.Internal.Socket.Action;
using Lagrange.XocMat.Internal.Socket.Action.Receive;
using Lagrange.XocMat.Internal.Socket.Action.Response;
using Lagrange.XocMat.Internal.Socket.PlayerMessage;
using Lagrange.XocMat.Internal.Socket.ServerMessage;
using Lagrange.XocMat.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace Lagrange.XocMat.Event;

public class TerrariaMsgReceiveHandler(BotContext bot, CommandManager cmdManager, TShockReceive receive, ILogger<TerrariaMsgReceiveHandler> logger)
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

    private Dictionary<PostMessageType, EventCallBack<ServerMsgArgs, ValueTask>> _action = [];

    private async ValueTask PlayerMessageHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<PlayerChatMessage>(args.Stream);
        if (OnPlayerChat != null)
            await OnPlayerChat(data);
        if (!data.Handler && data.TerrariaServer != null && !data.Mute)
        {
            if (data.Text.Length >= data.TerrariaServer.MsgMaxLength)
                return;
            foreach (var group in data.TerrariaServer.ForwardGroups)
            {
                await bot.SendMessage(MessageBuilder.Group(Convert.ToUInt32(group)).Text($"[{data.TerrariaServer.Name}] {data.Name}: {data.Text}").Build());
            }
        }
    }

    private async ValueTask HeartBeatHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<BaseMessage>(args.Stream);
        WebSocketConnectManager.Add(data.ServerName, args.id);
        if (OnHeartBeat != null)
            await OnHeartBeat(data);
        await ValueTask.CompletedTask;
    }

    private async ValueTask ConnectHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<BaseMessage>(args.Stream);
        WebSocketConnectManager.Add(data.ServerName, args.id);
        if (OnConnect != null) await OnConnect(data);
        logger.LogInformation($"Terraria Server {data.ServerName} {args.id} 已连接...", ConsoleColor.Green);
        await data.TerrariaServer!.ReplyConnectStatus();
    }

    private async ValueTask GamePostInitHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<GameInitMessage>(args.Stream);
        if (OnGameInit != null) await OnGameInit(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (var group in data.TerrariaServer.ForwardGroups)
            {
                await bot.SendMessage(MessageBuilder.Group(Convert.ToUInt32(group)).Text($"[{data.TerrariaServer.Name}]服务器初始化已完成..").Build());
            }
        }
    }

    private async ValueTask PlayerCommandHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<PlayerCommandMessage>(args.Stream);
        if (OnPlayerCommand != null) await OnPlayerCommand(data);
        if (!data.Handler)
        {
            await cmdManager.CommandAdapter(data);
        }
    }

    private async ValueTask PlayerLeaveHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<PlayerLeaveMessage>(args.Stream);
        if (OnPlayerLeave != null) await OnPlayerLeave(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (var group in data.TerrariaServer.ForwardGroups)
            {
                await bot.SendMessage(MessageBuilder.Group(Convert.ToUInt32(group)).Text($"[{data.TerrariaServer.Name}] {data.Name}离开服务器..").Build());
            }
        }
    }

    private async ValueTask PlayerJoinHandler(ServerMsgArgs args)
    {
        var data = Serializer.Deserialize<PlayerJoinMessage>(args.Stream);
        if (OnPlayerJoin != null) await OnPlayerJoin(data);
        if (!data.Handler && data.TerrariaServer != null)
        {
            foreach (var group in data.TerrariaServer.ForwardGroups)
            {
                await bot.SendMessage(MessageBuilder.Group(Convert.ToUInt32(group)).Text($"[{data.TerrariaServer.Name}] {data.Name}进入服务器..").Build());
            }
        }
    }

    private ValueTask ActionHandler(ServerMsgArgs args)
    {
        var msg = Serializer.Deserialize<BaseAction>(args.Stream);
        ApiSubject.OnNext((msg, args.Stream.ToArray()));
        return ValueTask.CompletedTask;
    }

    internal async ValueTask<T?> GetResponse<T>(string echo, TimeSpan? timeout = null) where T : BaseActionResponse, new()
    {
        try
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(15);
            var task = ApiSubject.Where(x => x.Item1.Echo == echo)
            .Select(x => x.Item2)
                .Timeout((TimeSpan)timeout)
                .Take(1)
                .ToTask();
            var buffer = await task;
            using var Stream = new MemoryStream(buffer);
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
            var baseMsg = Serializer.Deserialize<BaseMessage>(args.Stream);
            if (baseMsg.TerrariaServer != null
                && baseMsg.Token == baseMsg.TerrariaServer.Token
                && _action.TryGetValue(baseMsg.MessageType, out var action))
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
                var echo = Guid.NewGuid().ToString();
                using var ms = new MemoryStream();
                var response = new SocketConnectStatusArgs()
                {
                    ServerName = baseMsg.ServerName,
                    ActionType = ActionType.ConnectStatus,
                    Token = baseMsg.Token,
                    Echo = echo,
                };
                if (baseMsg.TerrariaServer == null)
                {
                    logger.LogError($"接受到{baseMsg.ServerName} 的连接请求但，在配置文件中没有找到{baseMsg.ServerName}服务器!");
                    response.Status = SocketConnentType.ServerNull;

                }
                else if (baseMsg.Token != baseMsg.TerrariaServer?.Token)
                {
                    response.Status = SocketConnentType.VerifyError;
                    logger.LogError($"{baseMsg.ServerName} 的Token 与配置文件不匹配!");
                }
                else
                {
                    logger.LogError($"{baseMsg.ServerName} 未知连接错误!");
                    response.Status = SocketConnentType.Error;
                }
                Serializer.Serialize(ms, response);
                await receive.Send(ms.ToArray(), args.ConnectId);
                await receive.Close(args.ConnectId, System.Net.WebSockets.WebSocketCloseStatus.NormalClosure);
                ms.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"解析信息是出现错误:{ex.Message}");
        }

    }

    internal void GroupMessageForwardAdapter(BotContext bot, GroupMessageEvent args)
    {
        var text = string.Join("", args.Chain.Where(c => c is TextEntity)
            .Select(t => ((TextEntity)t).Text));
        if (string.IsNullOrEmpty(text))
            return;

        var eventArgs = new GroupMessageForwardArgs()
        {
            Handler = false,
            GroupMessageEventArgs = args,
            Context = text,
        };
        if (OperatHandler.MessageForward(eventArgs).GetAwaiter().GetResult())
        {
            foreach (var server in XocMatAPI.Setting.Servers)
            {
                if (server != null && text.Length <= server.MsgMaxLength)
                {
                    if (server.ForwardGroups.Contains(Convert.ToUInt32(args.Chain.GroupUin)))
                    {
                        server.Broadcast($"[群消息][{args.Chain.GroupMemberInfo?.Uin}]: {text}", System.Drawing.Color.GreenYellow).GetAwaiter().GetResult();
                    }
                }
            }
        }

    }

    //internal async ValueTask GroupFile(GroupUpLoadFileEventArgs args)
    //{
    //    try
    //    {
    //        if (args.UpLoad.Size > 1024 * 1024 * 30)
    //            return;
    //        var (status, fileinfo) = await args.OneBotAPI.GetFile(args.UpLoad.ID);
    //        if (status.RetCode != MomoAPI.Enumeration.ApiType.ApiStatusType.Ok || string.IsNullOrEmpty(fileinfo.Base64))
    //            return;
    //        var buffer = Convert.FromBase64String(fileinfo.Base64);
    //        foreach (var server in MorMorAPI.Setting.Servers)
    //        {
    //            if (server != null && server.WaitFile != null)
    //            {
    //                if (server.Groups.Contains(args.GroupId))
    //                {
    //                    server.WaitFile.TrySetResult(buffer);
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        await args.OneBotAPI.SendGroupMessage(args.GroupId, "[GetFile] Error" + e.Message);
    //    }
    //}

    [MemberNotNull(nameof(_action))]
    public void Start()
    {
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
        receive.SocketMessage += Adapter;
        bot.Invoker.OnGroupMessageReceived += GroupMessageForwardAdapter;
    }
}

using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using Lagrange.XocMat.Configuration;
using Microsoft.Extensions.Logging;

namespace Lagrange.XocMat.Net;

public class WebSocketServer(ILogger<WebSocketServer> logger)
{

    public event Action<string>? OnConnect;

    public event Action<string, byte[]>? OnMessage;

    public event Action<string>? OnClose;

    private readonly HttpListener _listener = new();

    private readonly ConcurrentDictionary<string, ConnectionContext> _connections = [];

    public async ValueTask Start(CancellationToken token)
    {
        _listener.Prefixes.Add($"http://*:{XocMatSetting.Instance.SocketProt}/");
        _listener.Start();
        logger.LogInformation($"Websocket Start prot:{XocMatSetting.Instance.SocketProt}");
        try
        {
            while (true)
            {
                _ = HandleHttpListenerContext(await _listener.GetContextAsync().WaitAsync(token), token);
                token.ThrowIfCancellationRequested();
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError("WebSocket Error " + e.Message);
        }
    }

    public ConnectionContext? GetConnect(string id)
    {
        return !_connections.TryGetValue(id, out ConnectionContext? connection) ? null : connection;
    }

    private async Task HandleHttpListenerContext(HttpListenerContext context1, CancellationToken token)
    {
        string identifier = Guid.NewGuid().ToString();
        HttpListenerResponse response = context1.Response;
        try
        {
            if (!context1.Request.IsWebSocketRequest)
            {

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();

                return;
            }
            OnConnect?.Invoke(identifier);
            HttpListenerWebSocketContext wsContext = await context1.AcceptWebSocketAsync(null).WaitAsync(token);
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _connections.TryAdd(identifier, new(wsContext, cts, identifier));
            await ReceiveAsyncLoop(identifier, cts.Token);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            try
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Close();
            }
            catch
            {
                // ignored
            }
        }
    }

    public async ValueTask ReceiveAsyncLoop(string identifier, CancellationToken token)
    {
        if (!_connections.TryGetValue(identifier, out ConnectionContext? connection)) return;
        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int received = 0;
                while (true)
                {
                    ValueTask<ValueWebSocketReceiveResult> resultTask = connection.WsContext.WebSocket.ReceiveAsync(buffer.AsMemory(received), default);
                    ValueWebSocketReceiveResult result = !resultTask.IsCompleted ?
                        await resultTask.AsTask().WaitAsync(token) :
                        resultTask.Result;

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync(identifier, WebSocketCloseStatus.NormalClosure, token);
                        return;
                    }

                    received += result.Count;

                    if (result.EndOfMessage) break;

                    if (received == buffer.Length) Array.Resize(ref buffer, buffer.Length << 1);

                    token.ThrowIfCancellationRequested();
                }
                OnMessage?.Invoke(identifier, buffer.AsSpan(0, received).ToArray());
                token.ThrowIfCancellationRequested();
            }
        }
        catch (Exception e)
        {
            bool isCanceled = e is OperationCanceledException;

            WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure;
            CancellationToken t = default(CancellationToken);
            if (!isCanceled)
            {
                status = WebSocketCloseStatus.InternalServerError;
                t = token;
            }

            await DisconnectAsync(identifier, status, t);

            if (token.IsCancellationRequested) throw;
        }
        finally
        {
            connection.Cts.Cancel();
        }
    }

    public async ValueTask DisconnectAsync(string identifier, WebSocketCloseStatus status, CancellationToken token)
    {
        if (!_connections.TryRemove(identifier, out ConnectionContext? connection)) return;

        try
        {
            await connection.WsContext.WebSocket
                .CloseAsync(status, null, token)
                .WaitAsync(TimeSpan.FromSeconds(5), token);
            OnClose?.Invoke(identifier);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError("WebSocket Server Close Error " + e.Message);
        }
        finally
        {

            connection.Tcs.SetResult();
        }
    }

    public async ValueTask SendBytesAsync(byte[] payload, string identifier, CancellationToken token)
    {
        if (!_connections.TryGetValue(identifier, out ConnectionContext? connection)) return;

        await connection.SendSemaphoreSlim.WaitAsync(token);

        try
        {
            await connection.WsContext.WebSocket.SendAsync(payload.AsMemory(), WebSocketMessageType.Binary, true, token);
        }
        finally
        {
            connection.SendSemaphoreSlim.Release();
        }
    }


    public class ConnectionContext(WebSocketContext context, CancellationTokenSource cts, string id)
    {
        public WebSocketContext WsContext { get; } = context;
        public SemaphoreSlim SendSemaphoreSlim { get; } = new(1);
        public CancellationTokenSource Cts { get; } = cts;
        public TaskCompletionSource Tcs { get; } = new();

        public string ID { get; } = id;
    }
}

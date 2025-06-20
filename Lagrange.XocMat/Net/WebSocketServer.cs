using Lagrange.XocMat.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;

namespace Lagrange.XocMat.Net;

public class WebSocketServer(ILogger<WebSocketServer> logger)
{

    public event Func<string, Task>? OnConnect;

    public event Func<string, byte[], Task>? OnMessage;

    public event Func<string, Task>? OnClose;

    private readonly HttpListener _listener = new();

    private readonly ConcurrentDictionary<string, ConnectionContext> _connections = [];

    public async Task Start(CancellationToken token)
    {
        _listener.Prefixes.Add($"http://*:{XocMatSetting.Instance.SocketProt}/");
        _listener.Start();
        logger.LogInformation("[{Time}][WebsockServer] Start Server Prot:{Prot}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), XocMatSetting.Instance.SocketProt);
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
            logger.LogError("[{Time}] [WebsockServer] Server Start Error: {Message}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Message);
        }
    }

    public ConnectionContext? GetConnect(string id) => _connections.TryGetValue(id, out var context) ? context : null;

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
            CancellationToken t = default;
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

    public async ValueTask DisconnectAsync(string identifier, WebSocketCloseStatus status, CancellationToken token = default)
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
            logger.LogError("[{Time}] [WebsockServer] Server Close Connect Error {Message}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Message);
        }
        finally
        {

            connection.Tcs.SetResult();
        }
    }

    public async ValueTask SendBytesAsync(byte[] payload, string identifier, CancellationToken token = default)
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

    public async Task StopAsync(CancellationToken token = default)
    {
        var tasks = _connections.Values.Select(t => t.Tcs.Task).ToArray();
        await Task.WhenAll(tasks);
        _listener.Stop();
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

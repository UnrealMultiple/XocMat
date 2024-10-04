using System.Net.WebSockets;
using Lagrange.XocMat.EventArgs.Sockets;
using Microsoft.Extensions.Hosting;


namespace Lagrange.XocMat.Net;

public class TShockReceive(WebSocketServer Server)
{
    public delegate ValueTask SocketCallBack<in T>(T args) where T : BaseSocketArgs;

    public event SocketCallBack<SocketDisposeArgs>? SocketDispose;

    public event SocketCallBack<SocketConnectArgs>? SocketConnect;

    public event SocketCallBack<SocketReceiveMessageArgs>? SocketMessage;

    private readonly CancellationToken CancellationToken = new CancellationToken();

    public WebSocketServer.ConnectionContext? GetConnectionContext(string id) => Server.GetConnect(id);

    public ValueTask Send(byte[] buffer, string id) => Server.SendBytesAsync(buffer, id, CancellationToken);

    public ValueTask Close(string id, WebSocketCloseStatus status) => Server.DisconnectAsync(id, status, CancellationToken);

    public async ValueTask StartService()
    {
        Server.OnConnect += async (context) =>
        {
            if (SocketConnect != null)
                await SocketConnect(new(context));
        };

        Server.OnMessage += (context, buffer) =>
        {
            Task.Run(async () =>
            {
                if (SocketMessage != null)
                {
                    using var stream = new MemoryStream(buffer);
                    await SocketMessage(new(context, stream));
                    stream.Dispose();
                }
            });
        };

        Server.OnClose += (id) =>
        {
            if (SocketDispose != null)
                SocketDispose(new(id));
        };
        await Server.Start(CancellationToken);

    }

    public async Task Start()
    {
        await StartService();
    }
}

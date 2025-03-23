using Microsoft.Extensions.DependencyInjection;

namespace Lagrange.XocMat.Net;

internal class WebSocketConnectManager
{

    private static readonly Dictionary<string, string> Connect = [];

    public static void Add(string name, string id)
    {
        Connect[name] = id;
    }

    public static void Remove(string name)
    {
        Connect.Remove(name);
    }

    public static string? GetConnentId(string name)
    {
        Connect.TryGetValue(name, out string? id);
        return id;
    }

    public static WebSocketServer.ConnectionContext? GetConnent(string name)
    {
        return Connect.TryGetValue(name, out string? id)
            ? XocMatApp.Instance.Services.GetRequiredService<TShockReceive>().GetConnectionContext(id)
            : null;
    }

    public static async Task Send(byte[] buffer, string id)
    {
        await XocMatApp.Instance.Services.GetRequiredService<TShockReceive>().Send(buffer, id);
    }
}

using Lagrange.XocMat.Internal.Socket;

namespace Lagrange.XocMat.EventArgs.Sockets;

public class ServerMsgArgs
{
    public required BaseMessage BaseMessage { get; set; }

    public required MemoryStream Stream { get; set; }

    public required string id { get; set; }

}

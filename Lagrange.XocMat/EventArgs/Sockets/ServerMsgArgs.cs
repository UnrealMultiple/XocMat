using Lagrange.XocMat.Terraria.Protocol;

namespace Lagrange.XocMat.EventArgs.Sockets;

public class ServerMsgArgs
{
    public BaseMessage BaseMessage { get; set; }

    public string Identity { get; set; }

    public ServerMsgArgs(BaseMessage baseMessage, string identity)
    {
        BaseMessage = baseMessage;
        Identity = identity;
        if (baseMessage.TerrariaServer != null) baseMessage.TerrariaServer.ConnectIdentity = identity;
    }

}

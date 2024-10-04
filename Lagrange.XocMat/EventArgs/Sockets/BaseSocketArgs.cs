namespace Lagrange.XocMat.EventArgs.Sockets;

public class BaseSocketArgs(string connectid)
{
    public string ConnectId { get; init; } = connectid;
}

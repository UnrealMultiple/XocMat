using Lagrange.Core.Message;

namespace Lagrange.XocMat.EventArgs;

public class ReloadEventArgs : System.EventArgs
{
    public MessageBuilder Message { get; }

    public ReloadEventArgs(uint group)
    {
        Message = MessageBuilder.Group(group);
    }
}

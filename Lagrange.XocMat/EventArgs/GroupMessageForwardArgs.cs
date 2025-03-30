using Lagrange.Core.Event.EventArg;

namespace Lagrange.XocMat.EventArgs;

public class GroupMessageForwardArgs : System.EventArgs
{
    public required GroupMessageEvent GroupMessageEventArgs { get; init; }

    public string Context { get; init; } = string.Empty;

    public bool Handler { get; set; }
}

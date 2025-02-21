using Lagrange.XocMat.Internal.Socket.Internet;
using ProtoBuf;

namespace Lagrange.XocMat.Internal.Socket.Action.Response;

[ProtoContract]
public class QueryAccount : BaseActionResponse
{
    [ProtoMember(8)] public List<Account> Accounts { get; set; } = [];
}

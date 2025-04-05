using ProtoBuf;

namespace Lagrange.XocMat.Terraria.Protocol.Action.Receive;

[ProtoContract]
public class ResetServerArgs : BaseAction
{
    [ProtoMember(5)] public List<string> RestCommamd { get; set; } = [];

    [ProtoMember(6)] public string StartArgs { get; set; } = string.Empty;

    [ProtoMember(7)] public bool UseFile { get; set; }

    [ProtoMember(8)] public string FileName { get; set; } = string.Empty;

    [ProtoMember(9)] public byte[] FileBuffer { get; set; } = Array.Empty<byte>();
}



using System.Text.Json.Serialization;

namespace Lagrange.XocMat.Internal.Terraria;

public class Prize
{
    [JsonPropertyName("奖品名称")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("奖品ID")]
    public int ID { get; set; }

    [JsonPropertyName("中奖概率")]
    public int Probability { get; set; }

    [JsonPropertyName("最大数量")]
    public int Max { get; set; }

    [JsonPropertyName("最小数量")]
    public int Min { get; set; }
}

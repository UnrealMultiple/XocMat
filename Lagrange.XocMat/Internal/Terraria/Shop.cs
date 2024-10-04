using System.Text.Json.Serialization;

namespace Lagrange.XocMat.Internal.Terraria;

public class Shop
{
    [JsonPropertyName("商品名称")]
    public string Name { get; set; }

    [JsonPropertyName("商品ID")]
    public int ID { get; set; }

    [JsonPropertyName("商品价格 ")]
    public int Price { get; set; }

    [JsonPropertyName("商品数量")]

    public int num { get; set; }

    [JsonPropertyName("购买进度限制")]
    public string ProgressLimit { get; set; }

    public Shop(string name, int iD, int price, int num, string progressLimit = "")
    {
        Name = name;
        ID = iD;
        Price = price;
        this.num = num;
        ProgressLimit = progressLimit;
    }
}

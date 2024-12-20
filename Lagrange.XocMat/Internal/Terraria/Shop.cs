using Newtonsoft.Json;

namespace Lagrange.XocMat.Internal.Terraria;

public class Shop
{
    [JsonProperty("商品名称")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("商品ID")]
    public int ID { get; set; }

    [JsonProperty("商品价格 ")]
    public int Price { get; set; }

    [JsonProperty("商品数量")]

    public int Num { get; set; }

    [JsonProperty("购买进度限制")]
    public string ProgressLimit { get; set; } = string.Empty;

    public Shop(string name, int iD, int price, int num, string progressLimit = "")
    {
        Name = name;
        ID = iD;
        Price = price;
        Num = num;
        ProgressLimit = progressLimit;
    }

    public Shop()
    {

    }
}

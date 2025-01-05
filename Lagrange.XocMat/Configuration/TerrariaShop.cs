using Lagrange.XocMat.Attributes;
using Lagrange.XocMat.Internal.Terraria;
using Newtonsoft.Json;

namespace Lagrange.XocMat.Configuration;

[ConfigSeries]
public class TerrariaShop : JsonConfigBase<TerrariaShop>
{
    [JsonProperty("泰拉商店")]
    public List<Shop> TrShop { get; set; } = [];

    protected override string Filename => "Shop";

    public Shop? GetShop(string Name)
    {
        return TrShop.Find(x => x.Name == Name);
    }

    public Shop? GetShop(int id)
    {
        if (id > 0 && id <= TrShop.Count)
            return TrShop[id - 1];
        return null;
    }

    public bool TryGetShop(int id, out Shop? shop)
    {
        shop = GetShop(id);
        return shop != null;
    }

    public bool TryGetShop(string name, out Shop? shop)
    {
        shop = GetShop(name);
        return shop != null;
    }
}

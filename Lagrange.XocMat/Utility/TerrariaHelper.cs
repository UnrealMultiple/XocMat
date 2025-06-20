using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Terraria.Protocol.Internet;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Lagrange.XocMat.Utility;

public class TerrariaHelper
{
    public static Item? GetItemById(int id)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string file = "Lagrange.XocMat.Resources.Json.TerrariaID.json";
        Stream stream = assembly.GetManifestResourceStream(file)!;
        using StreamReader reader = new StreamReader(stream);
        JObject jobj = reader.ReadToEnd().ToObject<JObject>()!;
        JArray array = (JArray)jobj["物品"]!;
        foreach (JToken item in array)
        {
            if (item != null && item["ID"]!.Value<int>() == id)
            {
                return new()
                {
                    Name = item["中文名称"]!.Value<string>()!,
                    netID = id
                };
            }
        }
        return null;
    }

    public static List<Item> GetItemByName(string name)
    {
        List<Item> list = [];
        Assembly assembly = Assembly.GetExecutingAssembly();
        string file = "Lagrange.XocMat.Resources.Json.TerrariaID.json";
        Stream stream = assembly.GetManifestResourceStream(file)!;
        using StreamReader reader = new StreamReader(stream);
        JsonNode? jobj = JsonNode.Parse(reader.ReadToEnd());
        JsonArray array = jobj?["物品"]?.AsArray()!;
        foreach (JsonNode? item in array)
        {
            if (item != null && item["中文名称"]!.GetValue<string>().Contains(name))
            {
                list.Add(new()
                {
                    Name = item["中文名称"]!.GetValue<string>(),
                    netID = item["ID"]!.GetValue<int>()
                });
            }
        }
        return list;
    }

    public static List<Item> GetItemByIdOrName(string ji)
    {
        List<Item> list = [];
        if (int.TryParse(ji, out int i))
        {
            Item? item = GetItemById(i);
            if (item != null)
                list.Add(item);
        }
        else
        {
            list.AddRange(GetItemByName(ji));
        }
        return list;
    }
}

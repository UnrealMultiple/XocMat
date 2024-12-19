using Newtonsoft.Json;


namespace Lagrange.XocMat.Extensions;

public static class JsonExtension
{
    public static T? ToObject<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string ToJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}

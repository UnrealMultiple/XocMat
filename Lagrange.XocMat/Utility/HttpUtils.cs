using System.Text;
using System.Text.Json;
using System.Web;

namespace Lagrange.XocMat.Utility;

public static class HttpUtils
{
    private static readonly HttpClient HttpClient = new();
    public static async Task<string> HttpGetString(string url, Dictionary<string, string>? args = null)
    {
        return Encoding.UTF8.GetString(await HttpGetByte(url, args));
    }

    public static async Task<byte[]> HttpGetByte(string url, Dictionary<string, string>? args = null)
    {
        UriBuilder uriBuilder = new UriBuilder(url);
        System.Collections.Specialized.NameValueCollection param = HttpUtility.ParseQueryString(uriBuilder.Query);
        if (args != null)
            foreach ((string key, string val) in args)
                param[key] = val;
        uriBuilder.Query = param.ToString();
        return await HttpClient.GetByteArrayAsync(uriBuilder.ToString());
    }

    public static async Task<string> HttpPost(string url, Dictionary<string, string>? args = null)
    {
        FormUrlEncodedContent form = new(args ?? []);
        HttpResponseMessage content = await HttpClient.PostAsync(url, form);
        return await content.Content.ReadAsStringAsync();
    }

    public static async Task<string> HttpPostContent(string url, Dictionary<string, string> args)
    {
        StringContent payload = new(JsonSerializer.Serialize(args));
        HttpResponseMessage content = await HttpClient.PostAsync(url, payload);
        return await content.Content.ReadAsStringAsync();
    }
}

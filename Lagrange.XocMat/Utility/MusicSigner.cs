using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Lagrange.XocMat.Utility;

public class MusicSigner
{
    private static string? _signServer;

    private static readonly HttpClient _client = new();

    public MusicSigner(IConfiguration config, ILogger<MusicSigner> logger)
    {
        _signServer = config["MusicSignServerUrl"] ?? "";

        if (string.IsNullOrEmpty(_signServer))
        {
            logger.LogWarning("MusicSignServer is not available, sign may be failed");
        }
        else
        {
            logger.LogInformation("MusicSignServer Service is successfully established");
        }
    }


    public static string? Sign(Internal.MusicSigSegment musicSigSegment)
    {
        if (string.IsNullOrEmpty(_signServer)) return null;

        JsonObject payload;

        payload = new JsonObject()
        {
            { "type" , musicSigSegment.Type },
            { "url" , musicSigSegment.Url },
            { "audio" , musicSigSegment.Audio },
            { "title" , musicSigSegment.Title },
            { "image" , musicSigSegment.Image },
            { "singer" , musicSigSegment.Content },
        };
        try
        {
            HttpResponseMessage message = _client.PostAsJsonAsync(_signServer, payload).Result;
            return message.Content.ReadAsStringAsync().Result;
        }
        catch
        {
            return null;
        }
    }
}

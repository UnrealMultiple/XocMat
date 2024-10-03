

namespace Lagrange.XocMat.Internal;

public class MusicSigSegment(string type, string url, string Audio, string image, string title, string content)
{
    public string Type { get; set; } = type;

    public string Url { get; set; } = url;

    public string Audio { get; set; } = Audio;

    public string Title { get; set; } = title;

    public string Image { get; set; } = image;

    public string Content { get; set; } = content;
}

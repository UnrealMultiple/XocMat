using Lagrange.XocMat.Utility.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Terraria.Picture;

public class ProgressImage
{
    public static MemoryStream Start(Dictionary<string, bool> parameters, string serverName)
    {
        Random rand = new Random();
        int id = rand.Next(1, 30);
        using Image image = Image.Load($"Resources/Backgrounds/bg{id}.png");
        image.Mutate(x => x.Resize(4000, 3500));
        using Image slot = Image.Load($"Resources/Slot.png");
        ImageUtils.Instance.DrawProgresst(image, slot, parameters, 500, 400, maxLineCount: 7, darwCount: 28);
        ImageUtils.Instance.DrawText(image, $"{serverName}服务器", (image.Width / 2) - 300, 100, 150, Color.White);
        ImageUtils.Instance.ResetSize(image, 1500);
        using MemoryStream ms = new();
        image.SaveAsync(ms, new JpegEncoder());
        return ms;
    }

}

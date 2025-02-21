using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Utility.Images;

public class ImageMerger
{
    public async Task<byte[]> MergeImagesAsync(List<byte[]> imageBuffers, bool isHorizontal)
    {
        List<Image<Rgba32>> images = [];
        foreach (byte[] buffer in imageBuffers)
        {
            using MemoryStream ms = new MemoryStream(buffer);
            images.Add(await Image.LoadAsync<Rgba32>(ms));
        }

        int width = isHorizontal ? images.Sum(img => img.Width) : images.Max(img => img.Width);
        int height = isHorizontal ? images.Max(img => img.Height) : images.Sum(img => img.Height);

        using (Image<Rgba32> outputImage = new Image<Rgba32>(width, height))
        {
            int offset = 0;
            foreach (Image<Rgba32> img in images)
            {
                if (isHorizontal)
                {
                    outputImage.Mutate(ctx => ctx.DrawImage(img, new Point(offset, 0), 1f));
                    offset += img.Width;
                }
                else
                {
                    outputImage.Mutate(ctx => ctx.DrawImage(img, new Point(0, offset), 1f));
                    offset += img.Height;
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                await outputImage.SaveAsync(ms, new PngEncoder());
                return ms.ToArray();
            }
        }
    }
}

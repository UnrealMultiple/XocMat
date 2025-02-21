using Lagrange.XocMat.Internal.Socket.Internet;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Utility.Images;

internal class ImageUtils
{
    public FontFamily FontFamily { get; }

    public static readonly ImageUtils Instance = new();
    private ImageUtils()
    {
        FontCollection fc = new FontCollection();
        FontFamily = fc.Add("Resources/Font/simhei.ttf");
    }

    public void DrawImage(Image target, Image source, int X, int Y)
    {
        target.Mutate(x => x.DrawImage(source, new Point(X, Y), new GraphicsOptions()));
    }

    public void DrawText(Image image, string text, int x, int y, int fontSize, Color color)
    {
        Font font = new Font(FontFamily, fontSize);
        RichTextOptions textOptions = new(font)
        {
            Origin = new(x, y),
            TextAlignment = TextAlignment.Center
        };
        image.Mutate(ctx => ctx.DrawText(textOptions, text, color));
    }

    /// <summary>
    /// 等比例缩放
    /// </summary>
    /// <param name="image"></param>
    /// <param name="size"></param>
    public void ResetSize(Image image, int size)
    {
        int height = image.Height;
        int width = image.Width;
        if (height > width)
        {
            width = size * (width / height);
            image.Mutate(x => x.Resize(width, size));
        }
        else
        {
            height = size * (height / width);
            image.Mutate(x => x.Resize(size, height));
        }
    }

    public void DrawProgresst(Image image, Image slot, Dictionary<string, bool> progress, int x, int y, int slotSize = 400, int maxLineCount = 10, int darwCount = 50, bool erect = false)
    {
        ResetSize(slot, slotSize);
        Image<Rgba32> textSlot = slot.CloneAs<Rgba32>();
        textSlot.Mutate(x => x.Resize(textSlot.Width, 250));
        int sourceX = x;
        int sourceY = y;
        int intervalX = 40;
        int intervalY = 350;
        int i = 0;
        foreach ((string prog, bool status) in progress)
        {
            if (i >= darwCount)
                return;
            string res = $"Resources/Boss/{prog}.jpg";
            if (File.Exists(res))
            {

                DrawImage(image, slot, sourceX, sourceY);
                DrawImage(image, textSlot, sourceX, sourceY + slot.Height + intervalX);
                using Image itemPng = Image.Load(res);
                ResetSize(itemPng, slotSize - 40);
                DrawImage(image, itemPng, ((slot.Width - itemPng.Width) / 2) + sourceX, ((slot.Height - itemPng.Height) / 2) + sourceY);
                string text = status ? "已击杀" : "未击杀";
                Color color = status ? Color.GreenYellow : Color.White;
                DrawText(image, text, sourceX + 30, sourceY + slot.Height + intervalX + (textSlot.Height / 3), 110, color);
            }
            if ((i + 1) % maxLineCount == 0)
            {
                if (erect)
                {
                    sourceX += slotSize + intervalX;
                    sourceY = y;
                }
                else
                {
                    sourceY += slotSize + intervalY;
                    sourceX = x;
                }
            }
            else
            {
                if (erect)
                {
                    sourceY += slotSize + intervalY;
                }
                else
                {
                    sourceX += slotSize + intervalX;
                }
            }
            i++;
        }
    }

    /// <summary>
    /// 绘制物品卡槽
    /// </summary>
    /// <param name="image">图片资源</param>
    /// <param name="x">起始位置</param>
    /// <param name="y">起始位置</param>
    /// <param name="slotSize">卡槽大小</param>
    /// <param name="erect">竖直方向</param>
    public void DrawSlot(Image image, Image slot, Item[] items, int x, int y, int slotSize = 100, int maxLineCount = 10, int darwCount = 50, bool erect = false)
    {
        ResetSize(slot, slotSize);

        int sourceX = x;
        int sourceY = y;
        int interval = 10;
        for (int i = 0; i < darwCount; i++)
        {

            DrawImage(image, slot, sourceX, sourceY);
            if (items[i].stack > 0)
            {
                using Image itemPng = Image.Load($"Resources/Item/{items[i].netID}.png");
                ResetSize(itemPng, slotSize - 40);
                DrawImage(image, itemPng, ((slot.Width - itemPng.Width) / 2) + sourceX, ((slot.Height - itemPng.Height) / 2) + sourceY);
                DrawText(image, items[i].stack.ToString(), sourceX, sourceY + slot.Height - 30, 30, Color.White);
            }
            if ((i + 1) % maxLineCount == 0)
            {
                if (erect)
                {
                    sourceX += slotSize + interval;
                    sourceY = y;
                }
                else
                {
                    sourceY += slotSize + interval;
                    sourceX = x;
                }
            }
            else
            {
                if (erect)
                {
                    sourceY += slotSize + interval;
                }
                else
                {
                    sourceX += slotSize + interval;
                }

            }
        }
    }
}

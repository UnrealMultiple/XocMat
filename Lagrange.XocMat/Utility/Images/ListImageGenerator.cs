using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Utility.Images;

public class ListImageGenerator
{
    public Image Generate(
        List<string> items,
        Font font,
        Color textColor,
        Color bulletColor,
        int maxWidth = 800,
        Image? backgroundImage = null,
        Color? backgroundColor = null)
    {
        // 计算布局参数
        const int margin = 30;
        const int bulletRadius = 5;
        const int bulletTextSpacing = 10;
        const int itemSpacing = 15;
        const int lineSpacing = 4;

        // 测量文本布局
        TextOptions options = new TextOptions(font) { Dpi = 96 };
        int maxTextWidth = maxWidth - (margin * 2) - (bulletRadius * 2) - bulletTextSpacing;

        List<List<TextLayout>> layoutResults = [];
        float totalHeight = margin * 2;
        float maxContentWidth = 0;

        // 预计算所有文本布局
        foreach (string item in items)
        {
            List<string> lines = WrapText(item, maxTextWidth, font, options);
            List<TextLayout> lineLayouts = [];
            float itemHeight = 0;

            foreach (string line in lines)
            {
                FontRectangle size = TextMeasurer.MeasureSize(line, options);
                lineLayouts.Add(new TextLayout(line, size.Width, size.Height));
                itemHeight += size.Height + lineSpacing;
                maxContentWidth = Math.Max(maxContentWidth, size.Width);
            }

            layoutResults.Add(lineLayouts);
            totalHeight += itemHeight + itemSpacing;
        }

        // 计算最终尺寸
        int imageWidth = backgroundImage?.Width ?? Math.Min(maxWidth,
            (int)(maxContentWidth + (margin * 2) + (bulletRadius * 2) + bulletTextSpacing));
        int imageHeight = backgroundImage?.Height ?? (int)totalHeight;

        // 创建画布
        Image<Rgba32> image = new Image<Rgba32>(imageWidth, imageHeight);
        backgroundColor ??= Color.White;

        image.Mutate(ctx =>
        {
            // 绘制背景
            if (backgroundImage != null)
            {
                ctx.DrawImage(backgroundImage, new Point(0, 0), 1);
            }
            else
            {
                ctx.Fill(backgroundColor.Value);
            }

            // 绘制列表项
            float currentY = margin;
            foreach ((List<TextLayout> lineLayouts, int index) in layoutResults.Select((v, i) => (v, i)))
            {
                // 绘制圆点
                float bulletY = currentY + (lineLayouts.First().Height / 2);
                ctx.Fill(bulletColor,
                    new EllipsePolygon(margin + bulletRadius, bulletY, bulletRadius));

                // 绘制文本
                float currentLineY = currentY;
                foreach (TextLayout? layout in lineLayouts)
                {
                    ctx.DrawText(
                        new RichTextOptions(font)
                        {
                            Origin = new PointF(
                                margin + (bulletRadius * 2) + bulletTextSpacing,
                                currentLineY
                            ),
                            Dpi = 96,
                            WrappingLength = maxTextWidth
                        },
                        layout.Text,
                        textColor
                    );
                    currentLineY += layout.Height + lineSpacing;
                }

                currentY = currentLineY + itemSpacing;
            }
        });

        return image;
    }

    private List<string> WrapText(string text, float maxWidth, Font font, TextOptions options)
    {
        List<string> lines = [];
        string[] words = text.Split(' ');
        string currentLine = "";

        foreach (string word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            FontRectangle size = TextMeasurer.MeasureSize(testLine, options);

            if (size.Width <= maxWidth)
            {
                currentLine = testLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                }
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private record TextLayout(string Text, float Width, float Height);
}

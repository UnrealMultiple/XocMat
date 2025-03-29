using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Extensions;

public static class IImageProcessingContextExtensions
{
    public static void DrawRoundedRectangle(this IImageProcessingContext context, float x, float y, float width, float height, float cornerRadius, Rgba32 color)
    {
        if (cornerRadius <= 0)
        {
            // 如果没有圆角，就直接绘制矩形
            context.Fill(color, new RectangleF(x, y, width, height));
            return;
        }
        var radius = cornerRadius * 2;
        var pathBuilder = new PathBuilder();
        pathBuilder.StartFigure()
            .AddLine(x + cornerRadius, y, x + width - cornerRadius, y)
            .AddArc(new RectangleF(x + width - radius, y, radius, radius), 0, 270, 90)
            .AddLine(x + width, y + cornerRadius, x + width, y + height - cornerRadius)
            .AddArc(new RectangleF(x + width - radius, y + height - radius, radius, radius), 0, 0, 90)
            .AddLine(x + width - cornerRadius, y + height, x + cornerRadius, y + height)
            .AddArc(new RectangleF(x, y + height - radius, radius, radius), 0, 90, 90)
            .AddLine(x, y + height - cornerRadius, x, y + cornerRadius)
            .AddArc(new RectangleF(x, y, radius, radius), 0, 180, 90)
            .CloseFigure();
        var path = pathBuilder.Build();
        context.Fill(color, path);
    }

    public static void DrawRoundedRectanglePath(this IImageProcessingContext context, float x, float y, float width, float height, float cornerRadius, int size, Rgba32 color)
    {
        if (cornerRadius <= 0)
        {
            // 如果没有圆角，就直接绘制矩形
            context.Draw(color, size, new RectangleF(x, y, width, height));
            return;
        }
        var radius = cornerRadius * 2;
        var pathBuilder = new PathBuilder();
        pathBuilder.StartFigure()
            .AddLine(x + cornerRadius, y, x + width - cornerRadius, y)
            .AddArc(new RectangleF(x + width - radius, y, radius, radius), 0, 270, 90)
            .AddLine(x + width, y + cornerRadius, x + width, y + height - cornerRadius)
            .AddArc(new RectangleF(x + width - radius, y + height - radius, radius, radius), 0, 0, 90)
            .AddLine(x + width - cornerRadius, y + height, x + cornerRadius, y + height)
            .AddArc(new RectangleF(x, y + height - radius, radius, radius), 0, 90, 90)
            .AddLine(x, y + height - cornerRadius, x, y + cornerRadius)
            .AddArc(new RectangleF(x, y, radius, radius), 0, 180, 90)
            .CloseFigure();
        var path = pathBuilder.Build();
        context.Draw(color, size, path);
    }
}

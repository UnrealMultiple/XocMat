using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Utility.Images;

public class TableGenerator
{
    public async Task<byte[]> GenerateTable(TableBuilder tableBuilder)
    {
        string backgroundImagePath = tableBuilder.GetBackgroundImagePath();
        string title = tableBuilder.GetTitle();
        string[,] tableData = tableBuilder.GetTableData();
        Font font = tableBuilder.GetFont();
        bool titleBottom = tableBuilder.IsTitleBottom();

        // 计算每列的最大宽度
        int cellPadding = 10;
        int[] columnWidths = new int[tableData.GetLength(1)];
        for (int col = 0; col < tableData.GetLength(1); col++)
        {
            int maxWidth = 0;
            for (int row = 0; row < tableData.GetLength(0); row++)
            {
                FontRectangle textSize = TextMeasurer.MeasureSize(tableData[row, col], new TextOptions(font));
                maxWidth = Math.Max(maxWidth, (int)textSize.Width);
            }
            columnWidths[col] = maxWidth + (2 * cellPadding);
        }

        // 计算表格大小
        int tableWidth = 0;
        foreach (int width in columnWidths)
        {
            tableWidth += width;
        }
        int cellHeight = 40;
        int tableHeight = cellHeight * tableData.GetLength(0);

        // 如果标题长度大于表格宽度，则调整表格宽度
        TextOptions textOptions = new TextOptions(font) { HorizontalAlignment = HorizontalAlignment.Center };
        FontRectangle titleSize = TextMeasurer.MeasureSize(title, textOptions);
        int margin = 50;
        int titleMargin = 20;
        int titlePadding = 20; // 标题的额外边距
        if (titleSize.Width + (2 * titlePadding) > tableWidth)
        {
            // 计算每列的额外宽度
            int extraWidth = (int)(titleSize.Width + (2 * titlePadding) - tableWidth);
            int extraWidthPerColumn = extraWidth / tableData.GetLength(1);
            for (int col = 0; col < columnWidths.Length; col++)
            {
                columnWidths[col] += extraWidthPerColumn;
            }
            tableWidth = (int)titleSize.Width + (2 * titlePadding);
        }

        // 确保标题和边缘线之间留有一定的边距
        tableWidth += titleMargin * 2;

        // 计算标题的高度（考虑换行）
        string[] titleLines = title.Split('\n');
        int titleHeight = titleLines.Length * cellHeight;

        // 计算图片大小
        int imageWidth = tableWidth + (2 * margin);
        int imageHeight = tableHeight + (2 * margin) + titleHeight + 20; // + titleHeight for the title row

        // 检查是否提供了背景图片路径
        bool hasBackgroundImage = !string.IsNullOrEmpty(backgroundImagePath) && File.Exists(backgroundImagePath);

        // 加载背景图片或创建纯色背景
        using (Image<Rgba32> image = hasBackgroundImage ? Image.Load<Rgba32>(backgroundImagePath) : new Image<Rgba32>(imageWidth, imageHeight, Color.White))
        {
            // 如果使用背景图片，调整其大小
            if (hasBackgroundImage)
            {
                image.Mutate(ctx => ctx.Resize(imageWidth, imageHeight));
            }

            // 计算表格位置，使其居中
            int tableX = (imageWidth - tableWidth) / 2;
            int tableY = ((imageHeight - tableHeight) / 2) + titleHeight; // + titleHeight to account for the title row

            // 绘制半透明蒙层
            image.Mutate(ctx => ctx.Fill(new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = 0.5f } }, Color.White, new RectangleF(tableX, tableY, tableWidth, tableHeight)));

            // 绘制标题行
            int titleX = (imageWidth - (int)titleSize.Width) / 2;
            int titleY = tableY - titleHeight;
            DrawTextWithLineBreaks(image, title, font, Color.Black, new PointF(titleX, titleY + ((titleHeight - titleSize.Height) / 2)), tableWidth - (2 * titleMargin));

            // 绘制表格
            for (int row = 0; row < tableData.GetLength(0); row++)
            {
                int cellX = tableX + titleMargin;
                for (int col = 0; col < tableData.GetLength(1); col++)
                {
                    int cellY = tableY + (row * cellHeight);
                    string text = tableData[row, col];

                    // 设置文本居中
                    RichTextOptions newtextOptions = new RichTextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    // 计算文本位置
                    FontRectangle textSize = TextMeasurer.MeasureSize(text, newtextOptions);
                    float textX = cellX + ((columnWidths[col] - textSize.Width) / 2);
                    float textY = cellY + ((cellHeight - textSize.Height) / 2);

                    DrawTextWithLineBreaks(image, text, font, Color.Black, new PointF(textX, textY), columnWidths[col] - (2 * cellPadding));
                    cellX += columnWidths[col];
                }
            }

            // 绘制表格的列线和横线
            int currentX = tableX + titleMargin;
            for (int col = 0; col <= tableData.GetLength(1); col++)
            {
                image.Mutate(ctx => ctx.DrawLine(Color.Black, 1, new PointF(currentX, tableY), new PointF(currentX, tableY + tableHeight)));
                if (col < tableData.GetLength(1))
                {
                    currentX += columnWidths[col];
                }
            }

            for (int row = 0; row <= tableData.GetLength(0); row++)
            {
                int y = tableY + (row * cellHeight);
                image.Mutate(ctx => ctx.DrawLine(Color.Black, 1, new PointF(tableX + titleMargin, y), new PointF(tableX + tableWidth - titleMargin, y)));
            }

            // 绘制标题行的下边线和两边的列线
            int titleBottomY = tableY - titleHeight;
            if (titleBottom)
                image.Mutate(ctx =>
                {
                    ctx.DrawLine(Color.Black, 1, new PointF(tableX + titleMargin, titleBottomY), new PointF(tableX + tableWidth - titleMargin, titleBottomY));
                    ctx.DrawLine(Color.Black, 1, new PointF(tableX + titleMargin, titleBottomY), new PointF(tableX + titleMargin, titleBottomY + titleHeight));
                    ctx.DrawLine(Color.Black, 1, new PointF(tableX + tableWidth - titleMargin, titleBottomY), new PointF(tableX + tableWidth - titleMargin, titleBottomY + titleHeight));
                });

            // 保存结果
            using MemoryStream ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            return ms.ToArray();
        }
    }

    private void DrawTextWithLineBreaks(Image<Rgba32> image, string text, Font font, Color color, PointF position, float maxWidth)
    {
        string[] lines = text.Split('\n');
        float yOffset = 0;
        foreach (string line in lines)
        {
            FontRectangle textSize = TextMeasurer.MeasureSize(line, new TextOptions(font));
            image.Mutate(ctx => ctx.DrawText(line, font, color, new PointF(position.X, position.Y + yOffset)));
            yOffset += textSize.Height;
        }
    }
}

using Lagrange.XocMat.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lagrange.XocMat.Utility.Images;

public struct TableCell
{
    public string Text { get; set; }
    public Color TextColor { get; set; } = Color.Black;
    public Color BackgroundColor { get; set; } = Color.White;
    public bool UseTextColor { get; set; }

    public bool UseBackgroundColor { get; set; }

    public TableCell(string text, Color textColor, Color backgroundColor)
    {
        Text = text;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
        UseTextColor = true;
        UseBackgroundColor = true;
    }

    public TableCell(string text, Color textColor)
    {
        Text = text;
        TextColor = textColor;
        UseTextColor = true;
    }

    public TableCell(string text)
    {
        Text = text;
    }
}
public class TableContent
{
    public List<TableCell> Content { get; set; } = [];
}

public class TableBuilder
{
    public List<TableContent> Row { get; set; } = [];

    public List<TableCell> Header { get; set; } = [];

    private TableGenerate TableGenerate { get; set; } = new();

    public static TableBuilder Create() => new();

    public TableBuilder SetHeader(params string[] headers)
    {
        if (headers == null || headers.Length == 0) return this;
        foreach (var value in headers)
        {
            Header.Add(new TableCell(value));
        }
        return this;
    }

    public TableBuilder SetHeader(params TableCell[] headers)
    {
        if (headers == null || headers.Length == 0) return this;
        foreach (var value in headers)
        {
            Header.Add(value);
        }
        return this;
    }

    public TableBuilder AddRow(params string[] rows)
    {
        if (rows == null || rows.Length == 0) return this;

        var conten = new TableContent();
        foreach (var value in rows)
        {
            conten.Content.Add(new TableCell(value));
        }
        Row.Add(conten);
        return this;
    }

    public TableBuilder AddRow(params TableCell[] rows)
    {
        if (rows == null || rows.Length == 0) return this;

        var conten = new TableContent();
        foreach (var value in rows)
        {
            conten.Content.Add(value);
        }
        Row.Add(conten);
        return this;
    }

    public TableBuilder SetTitle(string title)
    {
        TableGenerate.Title = title;
        return this;
    }

    public TableBuilder SetLineMaxTextLength(int maxTextLength)
    {
        TableGenerate.LineMaxTextLength = maxTextLength;
        return this;
    }

    public TableBuilder SetMemberUin(uint memberUin)
    {
        TableGenerate.MemberUin = memberUin;
        return this;
    }

    public TableBuilder SetSignature(string signature)
    {
        TableGenerate.Signature = signature;
        return this;
    }

    public TableBuilder SetCardBackgroundColor(Color color)
    {
        TableGenerate.CardBackgroundColor = color;
        return this;
    }

    public TableBuilder SetTableFontColor(Color color)
    {
        TableGenerate.TableFontColor = color;
        return this;
    }

    public TableBuilder SetTitleColor(Color color)
    {
        TableGenerate.TitleColor = color;
        return this;
    }

    public TableBuilder SetSignatureColor(Color color)
    {
        TableGenerate.SignatureColor = color;
        return this;
    }

    public TableBuilder SetTableThicknessColor(Color color)
    {
        TableGenerate.TableThicknessColor = color;
        return this;
    }

    public TableBuilder SetFontSize(int fontSize)
    {
        TableGenerate.TableFontSize = fontSize;
        return this;
    }

    public TableBuilder SetGap(int gap)
    {
        TableGenerate.Gap = gap;
        return this;
    }

    public TableBuilder SetTableMargin(int tableMargin)
    {
        TableGenerate.TableMargin = tableMargin;
        return this;
    }

    public TableBuilder SetCardMargin(int cardMargin)
    {
        TableGenerate.CardMargin = cardMargin;
        return this;
    }

    public TableBuilder SetTableBottomMargin(int tableBottomMargin)
    {
        TableGenerate.TableBottomMargin = tableBottomMargin;
        return this;
    }

    public TableBuilder SetCardTopMargin(int cardTopMargin)
    {
        TableGenerate.CardTopMargin = cardTopMargin;
        return this;
    }

    public TableBuilder SetCardBottomMargin(int cardBottomMargin)
    {
        TableGenerate.CardBottomMargin = cardBottomMargin;
        return this;
    }

    public byte[] Builder()
    {
        if (Header.Count == 0) throw new Exception("you must set table header!");
        using var image = TableGenerate.DrawContent(this);
        return image.ToBytesAsync().Result;
    }
}

public class TableGenerate
{
    private string BackgroundPath => ImageUtils.GetRandOneBotBackground();

    public int TableFontSize { get; set; } = 26;

    public int TitleFontSize { get; set; } = 70;

    public int SignaturFontSize { get; set; } = 20;

    public int Gap { get; set; } = 20;

    public int LineMaxTextLength { get; set; } = 40;

    public int TableMargin { get; set; } = 50; // 表格与卡片边距

    public int CardMargin { get; set; } = 50; // 卡片与图片边距

    private int TableTopMargin { get; set; } = 400; // 表格上边距

    public int TableBottomMargin { get; set; } = 80; // 表格下边距

    public int CardTopMargin { get; set; } = 100; // 卡片上边距

    public int CardBottomMargin { get; set; } = 50; // 卡片下边距

    public string Title { get; set; } = "标题"; // 标题文本

    public string Signature { get; set; } = "Generated by Lagrange.XocMat";

    public uint MemberUin { get; set; } = 523321293; // 头像路径

    public int MinTableWidth { get; set; } = 800; // 表格最小宽度

    public Color CardBackgroundColor { get; set; } = Color.FromRgba(255, 255, 255, 230);

    public Color TableFontColor { get; set; } = Color.Black;

    public Color TitleColor { get; set; } = Color.Black;

    public Color SignatureColor { get; set; } = Color.DarkGray;

    public Color TableThicknessColor { get; set; } = Color.DarkGray;

    public (int[] RowHeigth, int[] RowWidth) ComputeLayout(TableBuilder builder)
    {
        var tableFont = GetFontFamily().CreateFont(TableFontSize);
        var textSize = TextMeasurer.MeasureSize("A", new TextOptions(tableFont));
        var textOption = new RichTextOptions(tableFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            WrappingLength = textSize.Width * LineMaxTextLength,
            WordBreaking = WordBreaking.BreakAll
        };
        var RowHeigths = new int[builder.Row.Count + 1]; // 包含表头
        var RowWidths = new int[builder.Header.Count];

        // 计算表头尺寸
        for (int j = 0; j < builder.Header.Count; j++)
        {
            var size = TextMeasurer.MeasureSize(builder.Header[j].Text, textOption);
            RowHeigths[0] = Math.Max(RowHeigths[0], (int)size.Height);
            RowWidths[j] = Math.Max(RowWidths[j], (int)size.Width);
        }

        // 计算表格内容尺寸
        for (int i = 0; i < builder.Row.Count; i++)
        {
            for (int j = 0; j < builder.Row[i].Content.Count; j++)
            {
                var size = TextMeasurer.MeasureSize(builder.Row[i].Content[j].Text, textOption);
                RowHeigths[i + 1] = Math.Max(RowHeigths[i + 1], (int)size.Height);
                RowWidths[j] = Math.Max(RowWidths[j], (int)size.Width);
            }
        }

        // 确保表格宽度不小于最小宽度
        var totalWidth = RowWidths.Sum() + 2 * Gap * builder.Header.Count;
        if (totalWidth < MinTableWidth)
        {
            var extraWidth = (MinTableWidth - totalWidth) / builder.Header.Count;
            for (int j = 0; j < RowWidths.Length; j++)
            {
                RowWidths[j] += extraWidth;
            }
        }

        return (RowHeigths, RowWidths.ToArray());
    }

    public Image<Rgba32> GetAvatar(int size) => ImageUtils.GetAvatar(MemberUin, size);

    public Image<Rgba32> DrawContent(TableBuilder builder)
    {
        using var background = Image.Load<Rgba32>(BackgroundPath);
        var fontFamily = GetFontFamily();
        var tableFont = fontFamily.CreateFont(TableFontSize);
        var titleFont = fontFamily.CreateFont(TitleFontSize);
        var signFont = fontFamily.CreateFont(SignaturFontSize);

        var textSize = TextMeasurer.MeasureSize("A", new TextOptions(tableFont));
        var (RowHeigth, RowWidths) = ComputeLayout(builder);
        var maxHeight = RowHeigth.Sum(i => i + 2 * Gap) + TableTopMargin + TableBottomMargin;
        var maxWidth = RowWidths.Sum(i => i + 2 * Gap) + 2 * TableMargin;
        var image = background.Crop(maxWidth + 2 * CardMargin, maxHeight + CardTopMargin + CardBottomMargin);

        image.Mutate(d =>
        {
            DrawBackground(d, maxWidth, maxHeight);
            DrawTitle(d, titleFont, maxWidth);
            DrawAvatar(d, maxWidth);
            DrawHeaderText(d, builder, tableFont, RowWidths, RowHeigth);
            DrawContentText(d, builder, tableFont, RowWidths, RowHeigth);
            DrawVerticalLines(d, builder, RowWidths, maxWidth, maxHeight);
            DrawHorizontalLines(d, builder, RowHeigth, maxWidth, maxHeight);
            DrawSignature(d, signFont, image.Width, maxHeight);
        });

        return image;
    }

    private void DrawBackground(IImageProcessingContext d, int maxWidth, int maxHeight)
    {
        d.DrawRoundedRectangle(CardMargin, CardTopMargin, maxWidth, maxHeight, 60, CardBackgroundColor);
    }

    private void DrawTitle(IImageProcessingContext d, Font titleFont, int maxWidth)
    {
        var titleSize = TextMeasurer.MeasureSize(Title, new TextOptions(titleFont));
        var titlePosition = new PointF(CardMargin + (maxWidth - titleSize.Width) / 2, CardTopMargin + 30);
        d.DrawText(Title, titleFont, TitleColor, titlePosition);
    }

    private void DrawAvatar(IImageProcessingContext d, int maxWidth)
    {
        var avatarSize = 200;
        using var avatar = GetAvatar(avatarSize);
        var avatarPosition = new Point(CardMargin + (maxWidth - avatarSize) / 2, CardTopMargin + 120);
        d.DrawImage(avatar, avatarPosition, 1);
    }

    private void DrawVerticalLines(IImageProcessingContext d, TableBuilder builder, int[] RowWidths, int maxWidth, int maxHeight)
    {
        int xOffset = CardMargin + TableMargin;
        for (int j = 0; j <= builder.Header.Count; j++)
        {
            d.DrawLine(TableThicknessColor, 1, new PointF(xOffset, CardTopMargin + TableTopMargin), new PointF(xOffset, CardTopMargin + TableTopMargin + maxHeight - TableTopMargin - TableBottomMargin));
            if (j < builder.Header.Count)
            {
                xOffset += RowWidths[j] + 2 * Gap;
            }
        }
    }

    private void DrawHorizontalLines(IImageProcessingContext d, TableBuilder builder, int[] RowHeigth, int maxWidth, int maxHeight)
    {
        int yOffset = CardTopMargin + TableTopMargin;
        for (int i = 0; i <= builder.Row.Count; i++)
        {
            d.DrawLine(TableThicknessColor, 1, new PointF(CardMargin + TableMargin, yOffset), new PointF(CardMargin + TableMargin + maxWidth - 2 * TableMargin, yOffset));
            if (i < builder.Row.Count)
            {
                yOffset += RowHeigth[i] + 2 * Gap;
            }
        }
        d.DrawLine(TableThicknessColor, 1, new PointF(CardMargin + TableMargin, CardTopMargin + TableTopMargin + maxHeight - TableTopMargin - TableBottomMargin), new PointF(CardMargin + TableMargin + maxWidth - 2 * TableMargin, CardTopMargin + TableTopMargin + maxHeight - TableTopMargin - TableBottomMargin));
    }

    private void DrawHeaderText(IImageProcessingContext d, TableBuilder builder, Font tableFont, int[] RowWidths, int[] RowHeigth)
    {
        int xOffset = CardMargin + TableMargin;
        var textSize = TextMeasurer.MeasureSize("A", new TextOptions(tableFont));
        for (int j = 0; j < builder.Header.Count; j++)
        {
            var cell = builder.Header[j];
            var cellRect = new RectangleF(xOffset, CardTopMargin + TableTopMargin, RowWidths[j] + 2 * Gap, RowHeigth[0] + 2 * Gap);
            if (cell.UseBackgroundColor)
            {
                d.Fill(cell.BackgroundColor, cellRect);
            }

            var textOption = new RichTextOptions(tableFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = textSize.Width * LineMaxTextLength,
                WordBreaking = WordBreaking.BreakAll,
                Origin = new PointF(xOffset + RowWidths[j] / 2 + Gap, CardTopMargin + TableTopMargin + RowHeigth[0] / 2 + Gap)
            };
            d.DrawText(textOption, cell.Text, cell.UseTextColor ? cell.TextColor : TableFontColor);
            xOffset += RowWidths[j] + 2 * Gap;
        }
    }

    private void DrawContentText(IImageProcessingContext d, TableBuilder builder, Font tableFont, int[] RowWidths, int[] RowHeigth)
    {
        int yOffset = CardTopMargin + TableTopMargin + RowHeigth[0] + 2 * Gap;
        var textSize = TextMeasurer.MeasureSize("A", new TextOptions(tableFont));
        for (int i = 0; i < builder.Row.Count; i++)
        {
            int xOffset = CardMargin + TableMargin;
            for (int j = 0; j < builder.Row[i].Content.Count; j++)
            {
                var cell = builder.Row[i].Content[j];
                var cellRect = new RectangleF(xOffset, yOffset, RowWidths[j] + 2 * Gap, RowHeigth[i + 1] + 2 * Gap);
                if (cell.UseBackgroundColor)
                {
                    d.Fill(cell.BackgroundColor, cellRect);
                }

                float textY = yOffset + RowHeigth[i + 1] / 2 + Gap;
                var textOption = new RichTextOptions(tableFont)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    WrappingLength = textSize.Width * LineMaxTextLength,
                    WordBreaking = WordBreaking.BreakAll,
                    Origin = new PointF(xOffset + RowWidths[j] / 2 + Gap, textY)
                };
                d.DrawText(textOption, cell.Text, cell.UseTextColor ? cell.TextColor : TableFontColor);
                xOffset += RowWidths[j] + 2 * Gap;
            }
            yOffset += RowHeigth[i + 1] + 2 * Gap;
        }
    }

    private void DrawSignature(IImageProcessingContext d, Font signFont, int imageWidth, int maxHeight)
    {
        var signTextOption = new RichTextOptions(signFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Origin = new PointF(imageWidth / 2, CardTopMargin + TableTopMargin + maxHeight - TableTopMargin - TableBottomMargin + 40)
        };
        d.DrawText(signTextOption, Signature, SignatureColor);
    }

    private FontFamily GetFontFamily()
    {
        return ImageUtils.Instance.FontFamily;
    }
}
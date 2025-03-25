﻿using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace Lagrange.XocMat.Utility.Images;


#region 表格相关类

public class TableStyle
{
    // 表格边框
    public bool ShowBorder { get; set; } = true;
    public Color BorderColor { get; set; } = Color.FromRgb(220, 220, 220);
    public float BorderThickness { get; set; } = 1.5f;

    // 表头样式
    public bool ShowHeader { get; set; } = true;
    public Color HeaderBackgroundColor { get; set; } = Color.FromRgb(240, 240, 240);
    public Color HeaderTextColor { get; set; } = Color.Black;
    
    public int MaxAutoWidth { get; set; } = 3000;
    public int MinAutoWidth { get; set; } = 600;

    // 行样式
    public Color RowBackgroundColor { get; set; } = Color.White;
    public Color AlternateRowBackgroundColor { get; set; } = Color.FromRgb(250, 250, 250);
    public bool UseAlternateRowColor { get; set; } = true;

    // 单元格样式
    public int CellPaddingLeft { get; set; } = 50;
    public int CellPaddingRight { get; set; } = 50;
    public int CellPaddingTop { get; set; } = 12;
    public int CellPaddingBottom { get; set; } = 12;

    // 表格圆角
    public float TableCornerRadius { get; set; } = 10;

    // 文本颜色
    public Color LabelTextColor { get; set; } = Color.DarkSlateGray;
    public Color ValueTextColor { get; set; } = Color.Black;

    // 自定义单元格首列宽度比例（0-1之间）
    public float FirstColumnWidthRatio { get; set; } = 0.4f;

    // 表格背景透明度（0-255，0为完全透明，255为完全不透明）
    public byte BackgroundOpacity { get; set; } = 0;

    // 默认文本对齐方式
    public HorizontalAlignment DefaultTextAlignment { get; set; } = HorizontalAlignment.Center;

    // 列宽比例（用于多列表格）
    public float[] ColumnWidthRatios { get; set; } = [];
    
    public bool AutoAdjustColumnWidth { get; set; } = true; // 是否自动调整列宽
    public float MinColumnWidthRatio { get; set; } = 0.1f;  // 列最小宽度比例(相对于表格总宽度)
    public float MaxColumnWidthRatio { get; set; } = 1f;  // 列最大宽度比例(相对于表格总宽度)
    public bool EnableTextWrapping { get; set; } = false;    // 即使自动调整列宽，也允许长文本换行
    public int MaxTextLines { get; set; } = 3;   
}

// 表格单元格
public class TableCell
{
    public string Text { get; set; }
    public Color TextColor { get; set; }
    public Color BackgroundColor { get; set; }
    public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;

    public TableCell(string text, Color textColor)
    {
        Text = text;
        TextColor = textColor;
        BackgroundColor = Color.Transparent; // 默认透明，使用行背景色
    }

    public TableCell(string text, Color textColor, Color backgroundColor, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        Text = text;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
        Alignment = alignment;
    }
}

// 表格行
public class TableRow
{
    public List<TableCell> Cells { get; } = [];
    public Color BackgroundColor { get; set; } = Color.Transparent; // 默认透明，使用表格默认行色

    public TableRow() { }

    public TableRow(params TableCell[] cells)
    {
        Cells.AddRange(cells);
    }
}

// 表格构建器 - 支持多列
public class TableBuilder(TableStyle? style = null)
{
    private readonly List<TableRow> rows = [];
    private readonly TableStyle style = style ?? new TableStyle();
    private readonly Color labelColor = style?.LabelTextColor ?? Color.DarkSlateGray;
    private readonly Color valueColor = style?.ValueTextColor ?? Color.Black;

    // 添加标准行（标签-值对）- 两列
    public TableBuilder AddRow(string label, string value)
    {
        var row = new TableRow(
            new TableCell(label, labelColor) { Alignment = style.DefaultTextAlignment },
            new TableCell(value, valueColor) { Alignment = style.DefaultTextAlignment }
        );
        rows.Add(row);
        return this;
    }

    // 添加带自定义颜色的行 - 两列
    public TableBuilder AddRow(string label, string value, Color labelColor, Color valueColor)
    {
        var row = new TableRow(
            new TableCell(label, labelColor) { Alignment = style.DefaultTextAlignment },
            new TableCell(value, valueColor) { Alignment = style.DefaultTextAlignment }
        );
        rows.Add(row);
        return this;
    }

    // 添加多列行
    public TableBuilder AddRow(params string[] values)
    {
        if (values == null || values.Length == 0) return this;

        var row = new TableRow();
        foreach (var value in values)
        {
            row.Cells.Add(new TableCell(value, valueColor) { Alignment = style.DefaultTextAlignment });
        }
        rows.Add(row);
        return this;
    }

    // 添加带自定义颜色的多列行
    public TableBuilder AddRow(string[] values, Color[] colors)
    {
        if (values == null || values.Length == 0) return this;

        var row = new TableRow();
        for (int i = 0; i < values.Length; i++)
        {
            var color = i < colors.Length ? colors[i] : valueColor;
            row.Cells.Add(new TableCell(values[i], color) { Alignment = style.DefaultTextAlignment });
        }
        rows.Add(row);
        return this;
    }

    // 添加完全自定义的行
    public TableBuilder AddCustomRow(TableRow row)
    {
        rows.Add(row);
        return this;
    }

    // 添加表头
    public TableBuilder AddHeader(params string[] headers)
    {
        if (headers != null && headers.Length > 0)
        {
            var headerRow = new TableRow();
            foreach (var header in headers)
            {
                headerRow.Cells.Add(new TableCell(header, style.HeaderTextColor) { Alignment = style.DefaultTextAlignment });
            }
            headerRow.BackgroundColor = style.HeaderBackgroundColor;
            rows.Insert(0, headerRow);
        }
        return this;
    }

    // 获取构建的表格行
    public List<TableRow> Build()
    {
        return [.. rows];
    }
}


#endregion

public class TableGenerate
{
    public uint AvatarPath { get; set; }
    public string BackgroundPath => "Resources/OneBotImage/1.jpg";
    public byte CardOpacity { get; set; } = 230;
    public int CardWidth { get; set; } = 1000;
    public float CardCornerRadius { get; set; } = 40;

    // 蒙层边距
    public int CardTopMargin { get; set; } = 100;
    public int CardBottomMargin { get; set; } = 100;

    // 内容边距
    public int ContentTopMargin { get; set; } = 40;
    public int ContentBottomMargin { get; set; } = 40;
    public int RowSpacing { get; set; } = 20;

    // 个人资料
    public string Title { get; set; } = "个人资料卡";
    public string Signature { get; set; } = "Generated by Lagrange.XocMat";
    public List<TableRow> TableRows { get; set; } = [];
    public TableStyle TableStyle { get; set; } = new TableStyle();

    public Color TitleColor { get; set; } = Color.Black;
    public Color SignatureColor { get; set; } = Color.DarkGray;

    // 字体大小
    public float TitleFontSize { get; set; } = 36;
    public float TableFontSize { get; set; } = 26;
    public float SmallFontSize { get; set; } = 20;

    // 头像设置
    public int AvatarSize { get; set; } = 120;
    public int AvatarBorderSize { get; set; } = 5;

    // 表格设置
    public int TableMarginTop { get; set; } = 30;
    public int TableMarginBottom { get; set; } = 30;

    // 主要生成函数
    public byte[] Generate()
    {
        try
        {
            // 创建头像
            using var avatar = CreateAvatar();

            // 计算尺寸和位置
            var layout = CalculateLayout(avatar.Height);
            
            // 使用动态计算的 CardWidth
            layout.CardWidth = Math.Clamp(layout.CardWidth, TableStyle.MinAutoWidth, TableStyle.MaxAutoWidth);
    

            // 准备背景
            using var background = PrepareBackground(layout.BackgroundWidth, layout.BackgroundHeight);

            // 创建卡片层
            using var cardBackground = CreateCardLayer(background, layout);

            // 将内容绘制到卡片上
            DrawContent(cardBackground, avatar, layout);

            using var ms = new MemoryStream();
            cardBackground.SaveAsPng(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成个人资料卡时出错: {ex.Message}");
            throw;
        }
    }

    #region 辅助函数


    // 获取字体
    private FontFamily GetFontFamily()
    {
        return ImageUtils.Instance.FontFamily;
    }

    // 创建头像
    private Image<Rgba32> CreateAvatar()
    {
        using Image<Rgba32> originalAvatar = Image.Load<Rgba32>(HttpUtils.GetByteAsync($"http://q.qlogo.cn/headimg_dl?dst_uin={AvatarPath}&spec=640&img_type=png").Result);
        originalAvatar.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(AvatarSize, AvatarSize),
            Mode = ResizeMode.Crop
        }));

        // 创建圆形头像
        using var circularAvatar = new Image<Rgba32>(AvatarSize, AvatarSize);
        circularAvatar.Mutate(x => {
            var circle = new EllipsePolygon(AvatarSize / 2, AvatarSize / 2, AvatarSize / 2);
            x.Clear(Color.Transparent);
            x.Fill(Color.White, circle);
            x.DrawImage(originalAvatar, new Point(0, 0), new GraphicsOptions
            {
                ColorBlendingMode = PixelColorBlendingMode.Multiply,
                AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn
            });
        });

        // 创建带边框的头像
        int finalAvatarSize = AvatarSize + (AvatarBorderSize * 2);
        var finalAvatar = new Image<Rgba32>(finalAvatarSize, finalAvatarSize);
        finalAvatar.Mutate(x => {
            var borderCircle = new EllipsePolygon(finalAvatarSize / 2, finalAvatarSize / 2, finalAvatarSize / 2);
            x.Fill(Color.White, borderCircle);
            x.DrawImage(circularAvatar, new Point(AvatarBorderSize, AvatarBorderSize), 1f);
        });

        return finalAvatar;
    }
    
    
    

    // 计算布局
    // 修改后的 CalculateLayout 方法
    private (int CardHeight, int CardWidth, int BackgroundWidth, int BackgroundHeight, int CardX, int CardY) CalculateLayout(int avatarHeight)
    {
        var fontFamily = GetFontFamily();
        var titleFont = fontFamily.CreateFont(TitleFontSize, FontStyle.Bold);
        var tableFont = fontFamily.CreateFont(TableFontSize, FontStyle.Regular);

        // ======== 动态宽度计算 ========
        // 计算标题实际宽度（包含边距）
        int titleWidth = !string.IsNullOrEmpty(Title) ? 
            (int)TextMeasurer.MeasureSize(Title, new TextOptions(titleFont)).Width + 60 : 0;

        // 计算表格动态宽度
        int tableContentWidth = CalculateTableWidth(tableFont);
    
        // 综合计算卡片宽度
        int dynamicCardWidth = Math.Max(titleWidth, tableContentWidth);
        dynamicCardWidth = Math.Clamp(
            dynamicCardWidth, 
            TableStyle.MinAutoWidth, 
            TableStyle.MaxAutoWidth - 100 // 保留安全边距
        );
        CardWidth = dynamicCardWidth;

        // ======== 动态高度计算 ========
        // 标题高度（包含上下间距）
        int titleHeight = !string.IsNullOrEmpty(Title) ? 
            (int)TextMeasurer.MeasureSize(Title, new TextOptions(titleFont)).Height + 40 : 0;

        // 头像区域高度
        int avatarAreaHeight = avatarHeight + 40;

        int maxRowHeight = CalculateMaxRowHeight(tableFont);
        int tableHeight = maxRowHeight * TableRows.Count
            + TableMarginTop
            + TableMarginBottom
            + (TableStyle.ShowBorder ? (int)(TableStyle.BorderThickness * 2) : 0);


        // 签名高度
        int signatureHeight = !string.IsNullOrEmpty(Signature) ? 
            (int)TextMeasurer.MeasureSize(Signature, new TextOptions(fontFamily.CreateFont(SmallFontSize))).Height + 20 : 0;

        int contentHeight = ContentTopMargin
           + titleHeight
           + avatarAreaHeight
           + tableHeight
           + signatureHeight
           + ContentBottomMargin;

        int backgroundHeight = contentHeight + CardTopMargin + CardBottomMargin;
        int cardHeight = contentHeight;
        int backgroundWidth = CardWidth + 100; // 增加边距弹性

        return (cardHeight, CardWidth, backgroundWidth, backgroundHeight,
                (backgroundWidth - CardWidth) / 2, CardTopMargin);
    }
    
    

    // 新增行高计算方法
    private int CalculateMaxRowHeight(Font tableFont)
    {
        int maxHeight = 0;
        var baseOptions = new TextOptions(tableFont);
        float sampleTextHeight = TextMeasurer.MeasureSize("测试", baseOptions).Height;

        foreach (var row in TableRows)
       {
           int rowHeight = 0;
           foreach (var cell in row.Cells)
           {
               var options = new TextOptions(tableFont)
               {
                   // 强制单行模式（不换行时直接限制为单行高度）
                   WrappingLength = TableStyle.EnableTextWrapping ? 
                       (float)(CardWidth * 0.8) : 
                       float.MaxValue,
                   WordBreaking = WordBreaking.Standard,
                   LayoutMode = LayoutMode.HorizontalTopBottom
               };
               
               var size = TextMeasurer.MeasureSize(cell.Text, options);
               // 无论是否换行，强制最多1行（不换行时直接取单行高度）
               int linesNeeded = TableStyle.EnableTextWrapping ? 
                   Math.Min((int)Math.Ceiling(size.Height / sampleTextHeight), TableStyle.MaxTextLines) : 
                   1; // 此处强制为1行
               
               int cellHeight = (int)(linesNeeded * sampleTextHeight) 
                   + TableStyle.CellPaddingTop 
                   + TableStyle.CellPaddingBottom;
               
               rowHeight = Math.Max(rowHeight, cellHeight);
           }
           maxHeight = Math.Max(maxHeight, rowHeight);
       }
       return maxHeight;
    }

    // 优化后的表格宽度计算
    private int CalculateTableWidth(Font tableFont)
    {
        if (TableRows == null || TableRows.Count == 0) return 600;

        var columnWidths = new Dictionary<int, int>();
        foreach (var row in TableRows)
        {
            for (int i = 0; i < row.Cells.Count; i++)
            {
                var cell = row.Cells[i];
                var size = TextMeasurer.MeasureSize(cell.Text, new TextOptions(tableFont));
                int cellWidth = (int)size.Width
                    + TableStyle.CellPaddingLeft
                    + TableStyle.CellPaddingRight
                    + (TableStyle.ShowBorder ? (int)TableStyle.BorderThickness : 0);

                columnWidths[i] = Math.Max(
                    columnWidths.ContainsKey(i) ? columnWidths[i] : 0,
                    cellWidth
                );
            }
        }

        // 增加最大宽度限制
        int maxAllowedWidth = TableStyle.MaxAutoWidth - 60;
        return Math.Min(columnWidths.Values.Sum(), maxAllowedWidth);
    }

    // 准备背景
    private Image<Rgba32> PrepareBackground(int width, int height)
    {
        using Image<Rgba32> originalBackground = Image.Load<Rgba32>(BackgroundPath);
        var option = new GraphicsOptions
        {
            Antialias = true,
            AntialiasSubpixelDepth = 16,
            BlendPercentage = 1,
            AlphaCompositionMode = PixelAlphaCompositionMode.Src
        };
        var background = new Image<Rgba32>(width, height);
        background.Mutate(x => x.SetGraphicsOptions(option));
        originalBackground.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

        background.Mutate(x => x.DrawImage(originalBackground, new Point(0, 0), 1f));

        return background;
    }

    // 创建卡片层
    private Image<Rgba32> CreateCardLayer(Image<Rgba32> background,
        (int CardHeight, int CardWidth, int BackgroundWidth, int BackgroundHeight, int CardX, int CardY) layout)
    {
        var cardBackground = new Image<Rgba32>(layout.BackgroundWidth, layout.BackgroundHeight);
        cardBackground.Mutate(x => {
            // 复制背景图像
            x.DrawImage(background, new Point(0, 0), 1f);

            // 创建圆角矩形卡片
            DrawRoundedRectangle(
                x,
                layout.CardX,
                layout.CardY,
                layout.CardWidth,
                layout.CardHeight,
                CardCornerRadius,
                new Rgba32(255, 255, 255, CardOpacity));
        });

        return cardBackground;
    }

    // 绘制圆角矩形
    private void DrawRoundedRectangle(IImageProcessingContext context, float x, float y, float width, float height, float cornerRadius, Rgba32 color)
    {
        if (cornerRadius <= 0)
        {
            // 如果没有圆角，就直接绘制矩形
            context.Fill(color, new RectangleF(x, y, width, height));
            return;
        }

        // 限制圆角大小
        var corners = Math.Min(cornerRadius, Math.Min(width, height) / 2);

        // 绘制四个角
        var leftTopCorner = new EllipsePolygon(x + corners, y + corners, corners);
        var rightTopCorner = new EllipsePolygon(x + width - corners, y + corners, corners);
        var rightBottomCorner = new EllipsePolygon(x + width - corners, y + height - corners, corners);
        var leftBottomCorner = new EllipsePolygon(x + corners, y + height - corners, corners);

        context.Fill(color, leftTopCorner);
        context.Fill(color, rightTopCorner);
        context.Fill(color, rightBottomCorner);
        context.Fill(color, leftBottomCorner);

        // 绘制四条边
        context.Fill(color, new RectangleF(x + corners, y, width - corners * 2, corners));  // 上边
        context.Fill(color, new RectangleF(x + width - corners, y + corners, corners, height - corners * 2));  // 右边
        context.Fill(color, new RectangleF(x + corners, y + height - corners, width - corners * 2, corners));  // 下边
        context.Fill(color, new RectangleF(x, y + corners, corners, height - corners * 2));  // 左边

        // 绘制中间区域
        context.Fill(color, new RectangleF(x + corners, y + corners, width - corners * 2, height - corners * 2));
    }

    // 绘制内容
    private void DrawContent(Image<Rgba32> canvas, Image<Rgba32> avatar,
        (int CardHeight, int CardWidth, int BackgroundWidth, int BackgroundHeight, int CardX, int CardY) layout)
    {
        var fontFamily = GetFontFamily();
        var titleFont = fontFamily.CreateFont(TitleFontSize, FontStyle.Bold);
        var tableFont = fontFamily.CreateFont(TableFontSize, FontStyle.Regular);
        var smallFont = fontFamily.CreateFont(SmallFontSize, FontStyle.Regular);

        canvas.Mutate(x => {
            int currentY = layout.CardY + ContentTopMargin;

            // 添加标题
            if (!string.IsNullOrEmpty(Title))
            {
                currentY = DrawTitle(x, titleFont, currentY, layout.BackgroundWidth);
            }

            // 添加头像
            currentY = DrawAvatar(x, avatar, currentY, layout.BackgroundWidth);

            // 添加信息表格
            currentY = DrawTable(x, tableFont, currentY, layout.CardX, layout.CardWidth);

            // 添加签名
            if (!string.IsNullOrEmpty(Signature))
            {
                DrawSignature(x, smallFont, currentY, layout.BackgroundWidth);
            }
        });
    }

    // 绘制标题
    private int DrawTitle(IImageProcessingContext context, Font titleFont, int currentY, int canvasWidth)
    {
        var titleOptions = new RichTextOptions(titleFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Origin = new PointF(canvasWidth / 2, currentY),
        };

        context.DrawText(titleOptions, Title, TitleColor);

        var titleHeight = (int)TextMeasurer.MeasureSize(Title, new TextOptions(titleFont)).Height;
        return currentY + titleHeight + 20;
    }

    // 绘制头像
    private int DrawAvatar(IImageProcessingContext context, Image avatar, int currentY, int canvasWidth)
    {
        int avatarX = canvasWidth / 2 - avatar.Width / 2;
        context.DrawImage(avatar, new Point(avatarX, currentY), 1f);

        return currentY + avatar.Height + 30;
    }


    private int DrawTable(IImageProcessingContext context, Font tableFont, int currentY, int cardX, int cardWidth)
    {
        // 表格的宽度和位置
        int tableMargin = 30;
        int tableWidth = cardWidth - (tableMargin * 2) - 20;
        int tableX = cardX + tableMargin;
        int tableY = currentY + TableMarginTop;

        // 如果没有数据，直接返回
        if (TableRows == null || TableRows.Count == 0)
        {
            return tableY;
        }

        // 计算基本文本高度
        var sampleTextHeight = TextMeasurer.MeasureSize("测试", new TextOptions(tableFont)).Height;
        int baseRowHeight = (int)(sampleTextHeight + TableStyle.CellPaddingTop + TableStyle.CellPaddingBottom);

        // 确定最大列数
        int maxColumns = TableRows.Max(row => row.Cells.Count);

        // 计算每列的理想宽度（基于内容）
        int[] idealColumnWidths = new int[maxColumns];
        int[] rowHeights = new int[TableRows.Count];

        // 初始化行高
        for (int i = 0; i < rowHeights.Length; i++)
        {
            rowHeights[i] = baseRowHeight;
        }

        // 首先计算每列的理想宽度
        if (TableStyle.AutoAdjustColumnWidth)
        {
            // 计算每个单元格的理想宽度
            for (int colIndex = 0; colIndex < maxColumns; colIndex++)
            {
                int maxWidth = 0;

                // 遍历每一行的该列
                foreach (var row in TableRows)
                {
                    if (colIndex < row.Cells.Count)
                    {
                        var cell = row.Cells[colIndex];
                        var textOptions = new TextOptions(tableFont);
                        var size = TextMeasurer.MeasureSize(cell.Text, textOptions);

                        // 为文本添加左右内边距
                        int cellWidth = (int)size.Width + TableStyle.CellPaddingLeft + TableStyle.CellPaddingRight;
                        maxWidth = Math.Max(maxWidth, cellWidth);
                    }
                }

                idealColumnWidths[colIndex] = maxWidth;
            }
        }

        // 计算每列实际宽度
        int[] columnWidths = new int[maxColumns];
        int totalIdealWidth = idealColumnWidths.Sum();

        // 最小和最大列宽限制
        int minColWidth = (int)(tableWidth * TableStyle.MinColumnWidthRatio);
        int maxColWidth = (int)(tableWidth * TableStyle.MaxColumnWidthRatio);

        if (totalIdealWidth <= tableWidth)
        {
            // 如果理想总宽度小于表格宽度，先确保每列至少达到最小宽度
            for (int i = 0; i < maxColumns; i++)
            {
                columnWidths[i] = Math.Max(idealColumnWidths[i], minColWidth);
            }

            // 分配剩余空间
            int allocatedWidth = columnWidths.Sum();
            if (allocatedWidth < tableWidth)
            {
                // 按比例分配剩余空间
                int remainingWidth = tableWidth - allocatedWidth;
                for (int i = 0; i < maxColumns; i++)
                {
                    double ratio = (double)idealColumnWidths[i] / totalIdealWidth;
                    columnWidths[i] += (int)(remainingWidth * ratio);
                }

                // 处理舍入误差
                allocatedWidth = columnWidths.Sum();
                columnWidths[maxColumns - 1] += tableWidth - allocatedWidth;
            }
        }
        else
        {
            // 如果理想总宽度大于表格宽度，按比例缩小
            for (int i = 0; i < maxColumns; i++)
            {
                double ratio = (double)idealColumnWidths[i] / totalIdealWidth;
                columnWidths[i] = (int)(tableWidth * ratio);

                // 应用最小和最大宽度限制
                columnWidths[i] = Math.Max(columnWidths[i], minColWidth);
                columnWidths[i] = Math.Min(columnWidths[i], maxColWidth);
            }

            // 调整列宽确保总宽度等于表格宽度
            int allocatedWidth = columnWidths.Sum();
            if (allocatedWidth != tableWidth)
            {
                // 按比例调整差额
                double adjustFactor = (double)tableWidth / allocatedWidth;
                for (int i = 0; i < maxColumns; i++)
                {
                    columnWidths[i] = (int)(columnWidths[i] * adjustFactor);
                }

                // 处理舍入误差
                allocatedWidth = columnWidths.Sum();
                columnWidths[maxColumns - 1] += tableWidth - allocatedWidth;
            }

            // 计算换行后的行高
            if (TableStyle.EnableTextWrapping)
            {
                for (int rowIndex = 0; rowIndex < TableRows.Count; rowIndex++)
                {
                    TableRow row = TableRows[rowIndex];
                    int maxRowHeight = baseRowHeight;

                    for (int cellIndex = 0; cellIndex < row.Cells.Count && cellIndex < maxColumns; cellIndex++)
                    {
                        var cell = row.Cells[cellIndex];
                        int availableTextWidth = columnWidths[cellIndex] - TableStyle.CellPaddingLeft - TableStyle.CellPaddingRight;

                        var textOptions = new TextOptions(tableFont)
                        {
                            WrappingLength = availableTextWidth,
                            WordBreaking = WordBreaking.BreakAll
                        };

                        var textSize = TextMeasurer.MeasureSize(cell.Text, textOptions);
                        int linesNeeded = Math.Min((int)Math.Ceiling(textSize.Height / sampleTextHeight), TableStyle.MaxTextLines);
                        int cellHeight = (int)(linesNeeded * sampleTextHeight + TableStyle.CellPaddingTop + TableStyle.CellPaddingBottom);
                        maxRowHeight = Math.Max(maxRowHeight, cellHeight);
                    }

                    rowHeights[rowIndex] = maxRowHeight;
                }
            }
        }

        // 重新计算表格总高度
        int totalHeight = rowHeights.Sum();

        // 绘制表格背景（带圆角和透明度）
        if (TableStyle.TableCornerRadius > 0)
        {
            // 使用半透明色绘制表格背景
            var backgroundColor = Color.FromRgba(
                255, 255, 255, TableStyle.BackgroundOpacity);

            DrawRoundedRectangle(
                context,
                tableX,
                tableY,
                tableWidth,
                totalHeight,
                TableStyle.TableCornerRadius,
                backgroundColor
            );
        }
        else
        {
            // 使用半透明色绘制表格背景
            var backgroundColor = Color.FromRgba(
                255, 255, 255, TableStyle.BackgroundOpacity);

            context.Fill(backgroundColor, new Rectangle(tableX, tableY, tableWidth, totalHeight));
        }

        // 绘制每一行
        int currentRowY = tableY;
        for (int rowIndex = 0; rowIndex < TableRows.Count; rowIndex++)
        {
            TableRow row = TableRows[rowIndex];
            int rowHeight = rowHeights[rowIndex];

            // 绘制行背景颜色（如果不是透明）
            Color rowBackgroundColor = row.BackgroundColor;

            // 如果行背景是透明的，则使用表格样式的行背景色
            if (rowBackgroundColor == Color.Transparent)
            {
                if (TableStyle.UseAlternateRowColor && rowIndex % 2 == 1)
                {
                    // 将交替行背景色应用透明度
                    rowBackgroundColor = TableStyle.AlternateRowBackgroundColor.WithAlpha(TableStyle.BackgroundOpacity);
                }
                else
                {
                    // 将普通行背景色应用透明度
                    var color = TableStyle.RowBackgroundColor;
                    rowBackgroundColor = color.WithAlpha(TableStyle.BackgroundOpacity);
                }
            }
            else
            {
                // 将自定义行背景色应用透明度
                rowBackgroundColor = rowBackgroundColor.WithAlpha(TableStyle.BackgroundOpacity);
            }

            // 为每一行绘制背景 - 需要注意第一行和最后一行的圆角
            if (TableStyle.TableCornerRadius > 0)
            {
                if (rowIndex == 0) // 第一行
                {
                    // 绘制带上圆角的行背景
                    DrawRoundedRectangleTopOnly(
                        context,
                        tableX, currentRowY,
                        tableWidth, rowHeight,
                        TableStyle.TableCornerRadius,
                        rowBackgroundColor
                    );
                }
                else if (rowIndex == TableRows.Count - 1) // 最后一行
                {
                    // 绘制带下圆角的行背景
                    DrawRoundedRectangleBottomOnly(
                        context,
                        tableX, currentRowY,
                        tableWidth, rowHeight,
                        TableStyle.TableCornerRadius,
                        rowBackgroundColor
                    );
                }
                else // 中间行
                {
                    // 普通矩形背景
                    context.Fill(rowBackgroundColor, new Rectangle(tableX, currentRowY, tableWidth, rowHeight));
                }
            }
            else
            {
                // 没有圆角，直接绘制矩形
                context.Fill(rowBackgroundColor, new Rectangle(tableX, currentRowY, tableWidth, rowHeight));
            }

            // 绘制单元格内容
            int currentColumnX = tableX;
            for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                if (cellIndex >= maxColumns) break;

                // 边界检查
                if (currentColumnX + columnWidths[cellIndex] > tableX + tableWidth)
                {
                    columnWidths[cellIndex] = tableX + tableWidth - currentColumnX;
                }
                var cell = row.Cells[cellIndex];
                int columnWidth = columnWidths[cellIndex];

                // 如果单元格有非透明背景色，绘制背景
                if (cell.BackgroundColor != Color.Transparent)
                {
                    // 应用透明度到单元格背景色
                    var cellBackgroundColor = cell.BackgroundColor.WithAlpha(TableStyle.BackgroundOpacity);

                    context.Fill(cellBackgroundColor, new Rectangle(currentColumnX, currentRowY, columnWidth, rowHeight));
                }

                // 设置居中对齐
                HorizontalAlignment alignment = TableStyle.DefaultTextAlignment; // 使用默认对齐方式

                // 如果单元格已经指定了自定义对齐方式，则使用该方式
                if (cell.Alignment != HorizontalAlignment.Left || TableStyle.DefaultTextAlignment == HorizontalAlignment.Left)
                {
                    alignment = cell.Alignment;
                }

                // 计算文本绘制位置
                float textX;
                switch (alignment)
                {
                    case HorizontalAlignment.Center:
                        textX = currentColumnX + (columnWidth / 2);
                        break;
                    case HorizontalAlignment.Right:
                        textX = currentColumnX + columnWidth - TableStyle.CellPaddingRight;
                        break;
                    default: // 左对齐
                        textX = currentColumnX + TableStyle.CellPaddingLeft;
                        break;
                }

                // 垂直居中，考虑可能的多行文本
                float textY = currentRowY + (rowHeight / 2) - (sampleTextHeight / 2);

                var textOptions = new RichTextOptions(tableFont)
                {
                    Origin = new PointF(textX, textY),
                    HorizontalAlignment = alignment,
                    WordBreaking = WordBreaking.Standard
                };

                // 如果启用文本换行并且行高够大
                if (TableStyle.EnableTextWrapping && rowHeight > baseRowHeight)
                {
                    textOptions.WrappingLength = columnWidth;
                    textOptions.WordBreaking = WordBreaking.Standard;

                    // 在多行情况下重新计算垂直位置
                    var textSize = TextMeasurer.MeasureSize(cell.Text, textOptions);
                    textY = currentRowY + (rowHeight / 2) - (textSize.Height / 2);
                    textOptions.Origin = new PointF(textX, textY);
                }

                context.DrawText(textOptions, cell.Text, cell.TextColor);
                currentColumnX += columnWidth;
            }

            // 更新行位置
            currentRowY += rowHeight;
        }

        // 绘制表格边框
        if (TableStyle.ShowBorder && TableStyle.BorderThickness > 0)
        {
            var borderPen = new SolidPen(TableStyle.BorderColor, TableStyle.BorderThickness);

            if (TableStyle.TableCornerRadius > 0)
            {
                DrawBorderWithCorners(context, tableX, tableY, tableWidth, totalHeight, TableStyle.TableCornerRadius, TableStyle.BorderColor, TableStyle.BorderThickness);

                // 绘制内部水平分隔线
                int currentLineY = tableY;
                for (int i = 0; i < TableRows.Count - 1; i++)
                {
                    currentLineY += rowHeights[i];
                    context.DrawLine(borderPen, new PointF[] {
                    new PointF(tableX, currentLineY),
                    new PointF(tableX + tableWidth, currentLineY)
                });
                }

                // 绘制内部垂直分隔线
                int colX = tableX;
                for (int i = 0; i < maxColumns - 1; i++)
                {
                    colX += columnWidths[i];
                    context.DrawLine(borderPen, new PointF[] {
                    new PointF(colX, tableY),
                    new PointF(colX, tableY + totalHeight)
                });
                }
            }
            else
            {
                context.DrawPolygon(borderPen, new PointF[] {
                new PointF(tableX, tableY),
                new PointF(tableX + tableWidth, tableY),
                new PointF(tableX + tableWidth, tableY + totalHeight),
                new PointF(tableX, tableY + totalHeight),
                new PointF(tableX, tableY)
            });

                // 绘制内部水平分隔线
                int currentLineY = tableY;
                for (int i = 0; i < TableRows.Count - 1; i++)
                {
                    currentLineY += rowHeights[i];
                    context.DrawLine(borderPen, new PointF[] {
                    new PointF(tableX, currentLineY),
                    new PointF(tableX + tableWidth, currentLineY)
                });
                }

                // 绘制内部垂直分隔线
                int colX = tableX;
                for (int i = 0; i < maxColumns - 1; i++)
                {
                    colX += columnWidths[i];
                    context.DrawLine(borderPen, new PointF[] {
                    new PointF(colX, tableY),
                    new PointF(colX, tableY + totalHeight)
                });
                }
            }
        }

        // 返回表格底部位置
        return currentRowY + TableMarginBottom;
    }



    // 只绘制顶部圆角的矩形
    private void DrawRoundedRectangleTopOnly(IImageProcessingContext context, float x, float y, float width, float height, float cornerRadius, Color color)
    {
        // 限制圆角大小
        var corners = Math.Min(cornerRadius, Math.Min(width, height) / 2);

        // 绘制顶部两个圆角
        var leftTopCorner = new EllipsePolygon(x + corners, y + corners, corners);
        var rightTopCorner = new EllipsePolygon(x + width - corners, y + corners, corners);

        context.Fill(color, leftTopCorner);
        context.Fill(color, rightTopCorner);

        // 绘制上边和两侧
        context.Fill(color, new RectangleF(x + corners, y, width - corners * 2, corners)); // 上边
        context.Fill(color, new RectangleF(x, y + corners, width, height - corners)); // 矩形主体
    }

    // 只绘制底部圆角的矩形
    private void DrawRoundedRectangleBottomOnly(IImageProcessingContext context, float x, float y, float width, float height, float cornerRadius, Color color)
    {
        // 限制圆角大小
        var corners = Math.Min(cornerRadius, Math.Min(width, height) / 2);

        // 绘制底部两个圆角
        var leftBottomCorner = new EllipsePolygon(x + corners, y + height - corners, corners);
        var rightBottomCorner = new EllipsePolygon(x + width - corners, y + height - corners, corners);

        context.Fill(color, leftBottomCorner);
        context.Fill(color, rightBottomCorner);

        // 绘制下边和两侧
        context.Fill(color, new RectangleF(x + corners, y + height - corners, width - corners * 2, corners)); // 下边
        context.Fill(color, new RectangleF(x, y, width, height - corners)); // 矩形主体
    }

    // 绘制带圆角的边框
    private void DrawBorderWithCorners(IImageProcessingContext context, float x, float y, float width, float height,
        float cornerRadius, Color borderColor, float borderThickness)
    {
        // 创建画笔
        var pen = new SolidPen(borderColor, borderThickness);
        var corners = Math.Min(cornerRadius, Math.Min(width, height) / 2);

        // 绘制边框线条 - 使用DrawLine方法和PointF数组

        // 上边线
        context.DrawLine(pen, [
            new PointF(x + corners, y),
            new PointF(x + width - corners, y)
        ]);

        // 右边线
        context.DrawLine(pen, [
            new PointF(x + width, y + corners),
            new PointF(x + width, y + height - corners)
        ]);

        // 下边线
        context.DrawLine(pen, [
            new PointF(x + width - corners, y + height),
            new PointF(x + corners, y + height)
        ]);

        // 左边线
        context.DrawLine(pen, [
            new PointF(x, y + height - corners),
            new PointF(x, y + corners)
        ]);

        // 左上角弧线（直线近似）
        context.DrawLine(pen, [
            new PointF(x, y + corners),
            new PointF(x + corners, y)
        ]);

        // 右上角弧线（直线近似）
        context.DrawLine(pen, [
            new PointF(x + width - corners, y),
            new PointF(x + width, y + corners)
        ]);

        // 右下角弧线（直线近似）
        context.DrawLine(pen, [
            new PointF(x + width, y + height - corners),
            new PointF(x + width - corners, y + height)
        ]);

        // 左下角弧线（直线近似）
        context.DrawLine(pen, new PointF[] {
            new PointF(x + corners, y + height),
            new PointF(x, y + height - corners)
        });
    }

    // 绘制签名
    private void DrawSignature(IImageProcessingContext context, Font smallFont, int currentY, int canvasWidth)
    {
        var signatureOptions = new RichTextOptions(smallFont)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Origin = new PointF(canvasWidth / 2, currentY + 10)
        };
        context.DrawText(signatureOptions, Signature, SignatureColor);
    }

    #endregion
}

﻿using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Utility.Images;

public class ProfileItem(string label, string value)
{
    public string Label { get; set; } = label;
    public string Value { get; set; } = value;
    public Color LabelColor { get; set; } = Color.DarkSlateGray;
    public Color ValueColor { get; set; } = Color.Black;
    public Color ValueBackgroundColor { get; set; } = Color.White;
    public bool UseEllipseBackground { get; set; } = true;
}

// 资料条目构建器
public class ProfileItemBuilder
{
    private readonly List<ProfileItem> items = [];

    // 添加基本条目
    public ProfileItemBuilder AddItem(string label, string value)
    {
        items.Add(new ProfileItem(label, value));
        return this;
    }

    // 添加自定义颜色的条目
    public ProfileItemBuilder AddItem(string label, string value, Color labelColor, Color valueColor, Color valueBackgroundColor)
    {
        var item = new ProfileItem(label, value)
        {
            LabelColor = labelColor,
            ValueColor = valueColor,
            ValueBackgroundColor = valueBackgroundColor
        };
        items.Add(item);
        return this;
    }

    // 添加特殊样式的条目
    public ProfileItemBuilder AddSpecialItem(string label, string value, bool useEllipseBackground)
    {
        var item = new ProfileItem(label, value)
        {
            UseEllipseBackground = useEllipseBackground
        };
        items.Add(item);
        return this;
    }

    // 获取构建好的条目列表
    public List<ProfileItem> Build()
    {
        return [.. items];
    }
}

public class ProfileCard
{
    // 配置选项
    public uint AvatarPath { get; set; }
    public string BackgroundPath => Directory.GetFiles("Resources/OneBotImage").Rand();
    public byte CardOpacity { get; set; } = 230;
    public int CardWidth { get; set; } = 450;
    public float CardCornerRadius { get; set; } = 40;

    // 蒙层边距
    public int CardTopMargin { get; set; } = 50;
    public int CardBottomMargin { get; set; } = 50;

    // 内容边距
    public int ContentTopMargin { get; set; } = 20;
    public int ContentBottomMargin { get; set; } = 20;
    public int RowSpacing { get; set; } = 50; // 行间距

    // 个人资料
    public List<ProfileItem> ProfileItems { get; set; } = [];
    public string Title { get; set; } = "个人资料卡";
    public string Signature { get; set; } = "Generated by Lagrange.XocMat";

    //// 字体设置
    //public string FontPath { get; set; } = "arial.ttf";
    public Color TitleColor { get; set; } = Color.Black;
    public Color SignatureColor { get; set; } = Color.DarkGray;

    // 默认颜色（对没有指定颜色的条目使用）
    public Color DefaultLabelColor { get; set; } = Color.DarkSlateGray;
    public Color DefaultValueColor { get; set; } = Color.Black;
    public Color DefaultValueBackgroundColor { get; set; } = Color.White;

    // 字体大小
    public float TitleFontSize { get; set; } = 26;
    public float NormalFontSize { get; set; } = 18;
    public float SmallFontSize { get; set; } = 14;

    // 头像设置
    public int AvatarSize { get; set; } = 120;
    public int AvatarBorderSize { get; set; } = 5;

    // 主要生成函数
    public byte[] Generate()
    {
        try
        {
            ValidateFiles();

            // 创建头像
            using var avatar = CreateAvatar();

            // 计算尺寸和位置
            var layout = CalculateLayout(avatar.Height);

            // 准备背景
            using var background = PrepareBackground(layout.BackgroundWidth, layout.BackgroundHeight);

            // 创建卡片层
            using var cardBackground = CreateCardLayer(background, layout);

            // 将内容绘制到卡片上
            DrawContent(cardBackground, avatar, layout);

            // 保存结果
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

    // 验证必要文件是否存在
    private void ValidateFiles()
    {
        if (!File.Exists(BackgroundPath)) throw new FileNotFoundException("背景文件不存在", BackgroundPath);
    }

    // 获取字体
    private FontFamily GetFontFamily()
    {
        return ImageUtils.Instance.FontFamily;
    }

    // 创建头像
    private Image<Rgba32> CreateAvatar()
    {
        // 加载并调整头像大小
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
    private (int CardHeight, int CardWidth, int BackgroundWidth, int BackgroundHeight, int CardX, int CardY)
        CalculateLayout(int avatarHeight)
    {
        var fontFamily = GetFontFamily();
        var titleFont = fontFamily.CreateFont(TitleFontSize, FontStyle.Bold);
        var normalFont = fontFamily.CreateFont(NormalFontSize, FontStyle.Regular);

        // 预先计算卡片所需高度
        int titleHeight = !string.IsNullOrEmpty(Title) ?
            (int)TextMeasurer.MeasureSize(Title, new TextOptions(titleFont)).Height + 30 : 0;
        int avatarAreaHeight = avatarHeight + 30; // 头像高度加上额外间距
        int itemAreaHeight = (ProfileItems.Count * RowSpacing) + 20; // 所有项目的高度
        int signatureHeight = !string.IsNullOrEmpty(Signature) ? 50 : 0; // 签名高度

        // 计算卡片内容总高度
        int contentHeight = ContentTopMargin + titleHeight + avatarAreaHeight + itemAreaHeight + signatureHeight + ContentBottomMargin;

        // 计算卡片总高度（内容高度 + 上下边距）
        int cardHeight = contentHeight;
        int cardWidth = CardWidth;

        // 计算背景图所需高度 (比卡片高度加上所需边距)
        int backgroundHeight = cardHeight + CardTopMargin + CardBottomMargin;
        int backgroundWidth = cardWidth + 50; // 左右各留25px边距

        // 计算卡片在背景中的位置
        int cardX = (backgroundWidth - cardWidth) / 2;
        int cardY = CardTopMargin;

        return (cardHeight, cardWidth, backgroundWidth, backgroundHeight, cardX, cardY);
    }

    // 准备背景
    private Image<Rgba32> PrepareBackground(int width, int height)
    {
        using Image<Rgba32> originalBackground = Image.Load<Rgba32>(BackgroundPath);

        // 创建背景画布
        var background = new Image<Rgba32>(width, height);

        // 调整原始背景大小并填充到新画布
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
        var normalFont = fontFamily.CreateFont(NormalFontSize, FontStyle.Regular);
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

            // 添加信息项
            currentY = DrawProfileItems(x, normalFont, currentY, layout.CardX, layout.CardWidth);

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
            Origin = new PointF(canvasWidth / 2, currentY)
        };

        context.DrawText(titleOptions, Title, TitleColor);

        var titleHeight = (int)TextMeasurer.MeasureSize(Title, new TextOptions(titleFont)).Height;
        return currentY + titleHeight + 20; // 返回新的Y坐标
    }

    // 绘制头像
    private int DrawAvatar(IImageProcessingContext context, Image avatar, int currentY, int canvasWidth)
    {
        int avatarX = canvasWidth / 2 - avatar.Width / 2;
        context.DrawImage(avatar, new Point(avatarX, currentY), 1f);

        return currentY + avatar.Height + 30; // 返回头像下方的Y坐标
    }

    // 绘制资料项
    private int DrawProfileItems(IImageProcessingContext context, Font normalFont, int currentY, int cardX, int cardWidth)
    {
        int leftMargin = cardX + 40;
        int rightMargin = cardX + cardWidth - 40;

        foreach (var item in ProfileItems)
        {
            // 获取颜色，如果没有自定义颜色则使用默认颜色
            var labelColor = item.LabelColor;
            var valueColor = item.ValueColor;
            var valueBackgroundColor = item.ValueBackgroundColor;

            // 标签 - 左对齐
            var labelOptions = new RichTextOptions(normalFont)
            {
                Origin = new PointF(leftMargin, currentY),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            context.DrawText(labelOptions, item.Label, labelColor);

            // 值文本 - 右对齐
            var valueTextSize = TextMeasurer.MeasureSize(item.Value, new TextOptions(normalFont));
            int paddingX = 15;
            int paddingY = 5;

            // 根据文本尺寸设置背景
            float backgroundWidth = valueTextSize.Width + (paddingX * 2);
            float backgroundHeight = valueTextSize.Height + (paddingY * 2);

            // 计算值的位置
            float valueTextX = rightMargin - backgroundWidth + paddingX;
            float valueTextY = currentY;

            // 根据配置绘制背景
            if (item.UseEllipseBackground)
            {
                // 椭圆背景
                float ellipseCenterX = rightMargin - (backgroundWidth / 2);
                float ellipseCenterY = currentY + (valueTextSize.Height / 2);

                var ellipse = new EllipsePolygon(ellipseCenterX, ellipseCenterY, backgroundWidth / 2, backgroundHeight / 2);
                context.Fill(valueBackgroundColor, ellipse);
            }
            else
            {
                // 矩形背景
                context.Fill(valueBackgroundColor, new RectangleF(
                    rightMargin - backgroundWidth,
                    currentY,
                    backgroundWidth,
                    backgroundHeight
                ));
            }

            // 绘制文本值
            var valueTextOptions = new RichTextOptions(normalFont)
            {
                Origin = new PointF(valueTextX, valueTextY),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            context.DrawText(valueTextOptions, item.Value, valueColor);

            currentY += RowSpacing;
        }

        return currentY; // 返回最后一项下方的Y坐标
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
}

using System.Globalization;
using System.Numerics;

namespace OmenTools.ImGuiOm;

/// <summary>
/// 保存当前字体的文本尺寸、图标字形顶部偏移及行推进距离，用于自定义绘制和居中布局。
/// 普通文本保留 ImGui 的布局尺寸，纯私用区图标使用可见字形的垂直边界。
/// </summary>
/// <param name="Size">文本布局宽度及用于对齐的高度，单位为屏幕像素。</param>
/// <param name="OffsetY">绘制原点到可见字形顶部的垂直偏移，单位为屏幕像素。</param>
public readonly record struct TextMetrics
(
    Vector2 Size,
    float   OffsetY
)
{
    /// <summary>获取相邻文本行之间的垂直推进距离，单位为屏幕像素。</summary>
    public float LineAdvance { get; private init; }

    /// <summary>获取度量时使用的最终字体大小，包含额外缩放。</summary>
    public float FontSize { get; private init; }

    /// <summary>
    /// 使用当前 ImGui 字体度量文本。结果应在同一字体作用域内使用；额外缩放同时作用于尺寸和字体大小。
    /// 文本不会自动换行，也不会隐藏双井号后的内容。
    /// </summary>
    /// <param name="text">需要度量的文本。</param>
    /// <param name="scale">相对于当前字体大小的缩放比例。</param>
    /// <returns>包含布局尺寸、垂直偏移、行推进距离及字体大小的度量结果。</returns>
    public static unsafe TextMetrics Measure(ReadOnlySpan<char> text, float scale = 1f)
    {
        var font       = ImGui.GetFont();
        var fontSize   = ImGui.GetFontSize();
        var glyphScale = fontSize / font.FontSize;
        var size       = ImGui.CalcTextSize(text);
        var lineY      = 0f;
        var minimumY   = float.PositiveInfinity;
        var maximumY   = float.NegativeInfinity;

        foreach (var character in text)
        {
            if (character == '\n')
            {
                lineY += fontSize;
                continue;
            }

            if (char.IsWhiteSpace(character))
                continue;

            if (char.GetUnicodeCategory(character) != UnicodeCategory.PrivateUse)
                return new(size * scale, 0)
                {
                    LineAdvance = fontSize * scale,
                    FontSize    = fontSize * scale
                };

            var glyph = font.FindGlyph(character);
            if (glyph->Y1 <= glyph->Y0)
                continue;

            minimumY = MathF.Min(minimumY, lineY + (glyph->Y0 * glyphScale));
            maximumY = MathF.Max(maximumY, lineY + (glyph->Y1 * glyphScale));
        }

        if (!float.IsFinite(minimumY))
            return new(size * scale, 0)
            {
                LineAdvance = fontSize * scale,
                FontSize    = fontSize * scale
            };

        return new(size with { Y = maximumY - minimumY } * scale, minimumY * scale)
        {
            LineAdvance = fontSize * scale,
            FontSize    = fontSize * scale
        };
    }

    /// <summary>计算文本在指定屏幕矩形内居中时的绘制原点，并补偿图标字形顶部偏移。</summary>
    /// <param name="origin">目标矩形左上角的屏幕坐标。</param>
    /// <param name="size">目标矩形的尺寸。</param>
    /// <returns>可直接传递给绘制列表的文本原点。</returns>
    public Vector2 CenterIn(Vector2 origin, Vector2 size) =>
        origin + ((size - Size) / 2f) - new Vector2(0, OffsetY);

    /// <summary>按度量时的字体大小居中绘制文本，并向当前布局提交占位区域。</summary>
    /// <param name="text">与度量结果对应的文本。</param>
    /// <param name="color">文本的打包颜色。</param>
    /// <param name="availableSize">布局占位尺寸；未指定时使用度量尺寸。</param>
    public void Draw(string text, uint color, Vector2? availableSize = null)
    {
        var size   = availableSize ?? Size;
        var origin = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), FontSize, CenterIn(origin, size), color, text);
        ImGui.Dummy(size);
    }

    /// <summary>绘制原生按钮背景，并按度量时的字体大小将文本或图标居中绘制。</summary>
    /// <param name="id">按钮在当前标识作用域内的唯一标识。</param>
    /// <param name="text">与度量结果对应的文本。</param>
    /// <param name="tooltip">悬停提示；为空时不显示。</param>
    /// <param name="availableSize">按钮尺寸；未指定时使用边长为当前控件高度的正方形。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public bool DrawButton(string id, string text, string tooltip = "", Vector2? availableSize = null)
    {
        var size    = availableSize ?? new Vector2(ImGui.GetFrameHeight());
        var origin  = ImGui.GetCursorScreenPos();
        var clicked = ImGui.Button($"##{id}", size);
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), FontSize, CenterIn(origin, size), ImGui.GetColorU32(ImGuiCol.Text), text);
        if (!string.IsNullOrEmpty(tooltip) &&
            ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);

        return clicked;
    }
}

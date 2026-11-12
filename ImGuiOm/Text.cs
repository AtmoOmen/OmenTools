using System.Numerics;
using Dalamud.Utility;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>按当前控件的文本基线绘制非格式化文本。</summary>
    /// <param name="text">需要显示的文本。</param>
    public static void Text(string text)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(text);
    }

    /// <summary>在同一行绘制图片及文字，并垂直居中两者。</summary>
    /// <param name="text">图片右侧的文本。</param>
    /// <param name="imageHandle">图片纹理句柄。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    public static void TextImage(string text, nint imageHandle, Vector2 imageSize) =>
        TextImage(text, new ImTextureID(imageHandle), imageSize);

    /// <summary>在同一行绘制图片及文字，并提交完整内容的布局占位。</summary>
    /// <param name="text">图片右侧的文本。</param>
    /// <param name="imageHandle">图片纹理标识。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    public static void TextImage(string text, ImTextureID imageHandle, Vector2 imageSize)
    {
        var metrics       = TextMetrics.Measure(text);
        var origin        = ImGui.GetCursorScreenPos();
        var spacing       = ImGui.GetStyle().ItemSpacing.X;
        var height        = MathF.Max(imageSize.Y, MathF.Max(metrics.Size.Y, metrics.LineAdvance));
        var imagePosition = origin + new Vector2(0,                     (height - imageSize.Y) / 2f);
        var textOrigin    = origin + new Vector2(imageSize.X + spacing, 0);
        var textPosition  = metrics.CenterIn(textOrigin, metrics.Size with { Y = height });
        var drawList      = ImGui.GetWindowDrawList();

        drawList.AddImage(imageHandle, imagePosition, imagePosition + imageSize, Vector2.Zero, Vector2.One, ImGui.GetColorU32(Vector4.One));
        drawList.AddText(textPosition, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.Dummy(new Vector2(imageSize.X + spacing + metrics.Size.X, height));
    }

    /// <summary>在当前可用宽度内水平居中绘制文本。</summary>
    /// <param name="text">需要显示的文本。</param>
    public static void TextCentered(string text)
    {
        CenterAlignFor(TextMetrics.Measure(text).Size.X);
        Text(text);
    }

    /// <summary>使用禁用样式绘制自动换行的文本。</summary>
    /// <param name="text">需要显示的文本。</param>
    public static void TextDisabledWrapped(string text)
    {
        using var disabled = ImRaii.Disabled();

        ImGui.TextWrapped(text);
    }

    /// <summary>使用禁用样式和指定换行宽度绘制文本。</summary>
    /// <param name="text">需要显示的文本。</param>
    /// <param name="warpPos">换行位置，相对于当前字体大小与界面缩放计算。</param>
    public static void TextDisabledWrapped(string text, float warpPos)
    {
        using var disabled = ImRaii.Disabled();
        using var warp     = ImRaii.TextWrapPos(ImGui.GetFontSize() * GlobalUIScale * warpPos);

        ImGui.TextUnformatted(text);
    }

    /// <summary>
    /// 在同一行绘制图标和文字，根据统一的文本度量在内容区域内垂直居中。
    /// </summary>
    /// <param name="icon">需要绘制的 FontAwesome 图标。</param>
    /// <param name="text">图标右侧的文字；为空时只绘制图标。</param>
    /// <param name="padding">内容的内边距；未指定时使用当前样式的控件内边距。</param>
    /// <param name="spacing">图标与文字的间隔，单位为屏幕像素；未指定时使用两倍水平内边距。</param>
    /// <returns>始终返回假，内容不响应点击。</returns>
    public static bool TextIcon
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2?        padding = null,
        float?          spacing = null
    )
    {
        var resolvedPadding = padding ?? ImGui.GetStyle().FramePadding;
        var resolvedSpacing = text.Length == 0 ?
                                  0f :
                                  spacing ?? 2f * resolvedPadding.X;
        var iconText       = icon.ToIconString();
        var textMetrics    = TextMetrics.Measure(text);
        var iconMetrics    = TextMetrics.Measure(iconText);
        var contentHeight  = MathF.Max(ImGui.GetFontSize(), MathF.Max(iconMetrics.Size.Y, textMetrics.Size.Y));
        var buttonSize     = new Vector2(iconMetrics.Size.X + textMetrics.Size.X + resolvedSpacing, contentHeight) + (2f * resolvedPadding);
        var contentOrigin  = ImGui.GetCursorScreenPos()                                                            + resolvedPadding;
        var iconPos        = iconMetrics.CenterIn(contentOrigin, iconMetrics.Size with { Y = contentHeight });
        var textOrigin     = contentOrigin + new Vector2(iconMetrics.Size.X + resolvedSpacing, 0);
        var textPos        = textMetrics.CenterIn(textOrigin, textMetrics.Size with { Y = contentHeight });
        var windowDrawList = ImGui.GetWindowDrawList();

        ImGui.Dummy(buttonSize);
        var color = ImGui.GetColorU32(ImGuiCol.Text);
        windowDrawList.AddText(iconPos, color, iconText);
        windowDrawList.AddText(textPos, color, text);

        return false;
    }

    /// <summary>在当前布局位置绘制带描边的文本。</summary>
    /// <param name="textColor">文本的打包颜色。</param>
    /// <param name="text">需要显示的文本。</param>
    /// <param name="outlineColor">描边的打包颜色。</param>
    /// <param name="outlineThickness">描边向外扩展的像素距离。</param>
    public static void TextOutlined(uint textColor, string text, uint outlineColor = 0xFF000000, float outlineThickness = 1.5f)
    {
        TextOutlined(ImGui.GetCursorScreenPos(), textColor, text, outlineColor, outlineThickness, ImGui.GetWindowDrawList());
        ImGui.Dummy(ImGui.CalcTextSize(text));
    }

    /// <summary>使用向量颜色在当前布局位置绘制带描边的文本。</summary>
    /// <param name="textColor">文本颜色。</param>
    /// <param name="text">需要显示的文本。</param>
    /// <param name="outlineColor">描边颜色；未指定时使用黑色。</param>
    /// <param name="outlineThickness">描边向外扩展的像素距离。</param>
    public static void TextOutlined
    (
        Vector4  textColor,
        string   text,
        Vector4? outlineColor     = null,
        float    outlineThickness = 1.5f
    ) =>
        TextOutlined
        (
            textColor.ToUInt(),
            text,
            outlineColor?.ToUInt() ?? 0xFF000000,
            outlineThickness
        );

    /// <summary>在指定屏幕坐标绘制带描边的文本，不提交布局占位。</summary>
    /// <param name="position">文本绘制原点。</param>
    /// <param name="textColor">文本的打包颜色。</param>
    /// <param name="text">需要显示的文本。</param>
    /// <param name="outlineColor">描边的打包颜色。</param>
    /// <param name="outlineThickness">描边向外扩展的像素距离。</param>
    /// <param name="drawList">目标绘制列表；未指定时使用背景绘制列表。</param>
    public static void TextOutlined
    (
        Vector2        position,
        uint           textColor,
        string         text,
        uint           outlineColor     = 0xFF000000,
        float          outlineThickness = 1.5f,
        ImDrawListPtr? drawList         = null
    )
    {
        var target = drawList ?? ImGui.GetBackgroundDrawList();

        for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
        for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
        {
            if (x == 0 && y == 0)
                continue;
            target.AddText(position + new Vector2(x, y), outlineColor, text);
        }

        target.AddText(position, textColor, text);
    }

    /// <summary>绘制可点击的文本链接，在点击后打开目标地址。</summary>
    /// <param name="url">目标地址。</param>
    /// <param name="displayText">显示文本；为空时显示地址。</param>
    /// <param name="showUnderline">是否绘制下划线。</param>
    /// <returns>链接被点击时返回真。</returns>
    public static bool TextLink(string url, string? displayText = null, bool showUnderline = true)
    {
        using var id    = ImRaii.PushId($"{url}_{displayText}_{showUnderline}");
        using var group = ImRaii.Group();

        var text = string.IsNullOrEmpty(displayText) ?
                       url :
                       displayText;

        var textSize  = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorScreenPos();

        var color      = ImGui.GetColorU32(ImGuiCol.Text);
        var hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);

        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);

        var clicked = false;
        if (ImGui.IsItemHovered())
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        TooltipHover(url, 40f);

        if (ImGui.IsItemClicked())
        {
            Util.OpenLink(url);
            clicked = true;
        }

        if (showUnderline)
        {
            const float underlineThickness = 1.0f;
            var         underlineStart     = cursorPos with { Y = cursorPos.Y + textSize.Y };
            var         underlineEnd       = new Vector2(cursorPos.X + textSize.X, cursorPos.Y + textSize.Y);

            var isHovered = ImGui.IsItemHovered();
            var underlineColor = isHovered ?
                                     hoverColor :
                                     color;

            ImGui.GetWindowDrawList().AddLine
            (
                underlineStart,
                underlineEnd,
                underlineColor,
                underlineThickness
            );
        }

        return clicked;
    }
}

using System.Numerics;
using Dalamud.Utility;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void Text(string text)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
        ImGui.TextUnformatted(text);
    }

    public static void TextImage(string text, nint imageHandle, Vector2 imageSize) =>
        TextImage(text, new ImTextureID(imageHandle), imageSize);

    public static void TextImage(string text, ImTextureID imageHandle, Vector2 imageSize)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        ImGui.Image(imageHandle, imageSize);

        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().FramePadding.Y);
        Text(text);
    }

    public static void TextCentered(string text)
    {
        CenterAlignFor(ImGui.CalcTextSize(text).X);
        Text(text);
    }

    public static void TextDisabledWrapped(string text)
    {
        using var disabled = ImRaii.Disabled();

        ImGui.TextWrapped(text);
    }

    public static void TextDisabledWrapped(string text, float warpPos)
    {
        using var disabled = ImRaii.Disabled();
        using var warp     = ImRaii.TextWrapPos(ImGui.GetFontSize() * GlobalUIScale * warpPos);

        Text(text);
    }

    /// <summary>
    /// 在同一行绘制图标和文字，根据图标与文字首字的可见字形中心进行垂直对齐。
    /// </summary>
    /// <param name="icon">需要绘制的 FontAwesome 图标。</param>
    /// <param name="text">图标右侧的文字；为空时只绘制图标。</param>
    /// <param name="padding">内容的内边距；未指定时使用当前样式的控件内边距。</param>
    /// <param name="spacing">图标与文字的间隔，单位为屏幕像素；未指定时使用两倍水平内边距。</param>
    /// <returns>保留原有禁用按钮的返回值，不响应点击。</returns>
    public static unsafe bool TextIcon
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2?        padding = null,
        float?          spacing = null
    )
    {
        var resolvedPadding = padding ?? ImGui.GetStyle().FramePadding;
        var resolvedSpacing = spacing ?? 2f * resolvedPadding.X;
        var iconText        = icon.ToIconString();
        var textFont        = ImGui.GetFont();
        var textFontSize    = ImGui.GetFontSize();
        var textSize        = ImGui.CalcTextSize(text);
        var textCenterY     = textSize.Y / 2f;

        if (text.Length > 0)
        {
            var glyph = textFont.FindGlyph(text[0]);
            textCenterY = (glyph->Y0 + glyph->Y1) / 2f * (textFontSize / textFont.FontSize);
        }

        var iconGlyph   = textFont.FindGlyph((ushort)icon);
        var iconSize    = ImGui.CalcTextSize(iconText);
        var iconCenterY = (iconGlyph->Y0 + iconGlyph->Y1) / 2f * (textFontSize / textFont.FontSize);

        var cursorPos    = ImGui.GetCursorScreenPos();
        var buttonHeight = Math.Max(iconSize.Y, textSize.Y);
        var iconOffsetY = text.Length > 0 ?
                              textCenterY - iconCenterY :
                              0f;
        var iconPos        = cursorPos + resolvedPadding + new Vector2(0,                            iconOffsetY);
        var textPos        = cursorPos + resolvedPadding + new Vector2(iconSize.X + resolvedSpacing, 0);
        var windowDrawList = ImGui.GetWindowDrawList();

        bool result;
        using (ImRaii.Disabled())
        using (ImRaii.PushColor(ImGuiCol.Button, 0))
            result = ImGui.Button(string.Empty, new(iconSize.X + textSize.X + resolvedSpacing + resolvedPadding.X, buttonHeight + (2f * resolvedPadding.Y)));

        windowDrawList.AddText(textFont, textFontSize, iconPos, ImGui.GetColorU32(ImGuiCol.Text), iconText);

        windowDrawList.AddText(textFont, textFontSize, textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static void TextOutlined(uint textColor, string text, uint outlineColor = 0xFF000000, float outlineThickness = 1.5f)
    {
        var originalPos = ImGui.GetCursorPos();

        using (ImRaii.Group())
        {
            // 8 方向阴影
            for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
            for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
            {
                if (x == 0 && y == 0) continue;

                ImGui.SetCursorPos(originalPos + new Vector2(x, y));
                using (ImRaii.PushColor(ImGuiCol.Text, outlineColor))
                    ImGui.TextUnformatted(text);
            }

            // 原始文字
            ImGui.SetCursorPos(originalPos);
            using (ImRaii.PushColor(ImGuiCol.Text, textColor))
                ImGui.TextUnformatted(text);
        }
    }

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
        drawList ??= ImGui.GetBackgroundDrawList();

        for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
        for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
        {
            if (x == 0 && y == 0) continue;
            drawList?.AddText(position + new Vector2(x, y), outlineColor, text);
        }

        drawList?.AddText(position, textColor, text);
    }

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

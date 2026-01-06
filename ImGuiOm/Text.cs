using System.Numerics;
using Dalamud.Interface;
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
        using var warp     = ImRaii.TextWrapPos(ImGui.GetFontSize() * warpPos);
        
        Text(text);
    }

    public static bool TextIcon(FontAwesomeIcon icon, string text, bool useStaticFont = false)
    {
        var iconSize = Vector2.Zero;
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            iconSize = ImGui.CalcTextSize(icon.ToIconString());

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos      = ImGui.GetCursorScreenPos();
        var padding        = ImGui.GetStyle().FramePadding;
        var textSize       = ImGui.CalcTextSize(text);
        var buttonHeight   = Math.Max(iconSize.Y, textSize.Y);

        var result = false;
        
        using (ImRaii.Disabled())
        using (ImRaii.PushColor(ImGuiCol.Button, 0))
            result = ImGui.Button(string.Empty, new(iconSize.X + textSize.X + (3 * padding.X), buttonHeight + (2 * padding.Y)));

        var iconPos = new Vector2(cursorPos.X + padding.X, cursorPos.Y + padding.Y);

        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        var textPos = new Vector2(iconPos.X + iconSize.X + (2 * padding.X), cursorPos.Y + padding.Y);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static void TextOutlined(Vector4 textColor, string text, Vector4 outlineColor = default, float outlineThickness = 1.5f)
    {
        if (outlineColor == default)
            outlineColor = new Vector4(0, 0, 0, 1);

        var originalPos = ImGui.GetCursorPos();
        using (ImRaii.Group())
        {
            // 8 方向阴影
            for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
            {
                for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
                {
                    if (x == 0 && y == 0) continue;

                    ImGui.SetCursorPos(originalPos + new Vector2(x, y));
                    ImGui.TextColored(outlineColor, text);
                }
            }

            // 原始文字
            ImGui.SetCursorPos(originalPos);
            ImGui.TextColored(textColor, text);
        }
    }

    public static void TextOutlined(
        Vector2        position,
        uint           textColor,
        string         text,
        uint           outlineColor     = 0xFF000000,
        float          outlineThickness = 1.5f,
        ImDrawListPtr? drawList         = null)
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
        {
            for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
            {
                if (x == 0 && y == 0) continue;
                drawList?.AddText(position + new Vector2(x, y), outlineColor, text);
            }
        }

        drawList?.AddText(position, textColor, text);
    }

    public static bool TextLink(string url, string? displayText = null, bool showUnderline = true)
    {
        using var id    = ImRaii.PushId($"{url}_{displayText}_{showUnderline}");
        using var group = ImRaii.Group();
        
        var text = string.IsNullOrEmpty(displayText) ? url : displayText;

        var textSize  = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorScreenPos();

        var color      = ImGui.GetColorU32(ImGuiCol.Text);
        var hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);

        ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(color), text);
        
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
            var         underlineEnd       = new Vector2(cursorPos.X              + textSize.X, cursorPos.Y + textSize.Y);

            var isHovered      = ImGui.IsItemHovered();
            var underlineColor = isHovered ? hoverColor : color;

            ImGui.GetWindowDrawList().AddLine(
                underlineStart,
                underlineEnd,
                underlineColor,
                underlineThickness);
        }

        return clicked;
    }
}

using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void Text(string text)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
        ImGui.Text(text);
    }

    public static void TextImage(string text, nint imageHandle, Vector2 imageSize)
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
        ImGui.BeginDisabled();
        ImGui.TextWrapped(text);
        ImGui.EndDisabled();
    }

    public static void TextDisabledWrapped(string text, float warpPos)
    {
        ImGui.BeginDisabled();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * warpPos);
        Text(text);
        ImGui.PopTextWrapPos();
        ImGui.EndDisabled();
    }

    public static bool TextIcon(FontAwesomeIcon icon, string text, bool useStaticFont = false)
    {
        if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        if (useStaticFont) ImGui.PopFont();

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding;

        var textSize = ImGui.CalcTextSize(text);
        var buttonHeight = Math.Max(iconSize.Y, textSize.Y);

        ImGui.BeginDisabled();
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = ImGui.Button("", new Vector2(iconSize.X + textSize.X + 3 * padding.X, buttonHeight + 2 * padding.Y));
        ImGui.PopStyleColor();
        ImGui.EndDisabled();

        var iconPos = new Vector2(cursorPos.X + padding.X, cursorPos.Y + padding.Y);
        if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        if (useStaticFont) ImGui.PopFont();

        var textPos = new Vector2(iconPos.X + iconSize.X + 2 * padding.X, cursorPos.Y + padding.Y);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static void TextOutlined(
        Vector2 position, uint textColor, string text, uint outlineColor = 0xFF000000, float outlineThickness = 1.5f,
        ImDrawListPtr? drawList = null)
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        // 8 方向阴影
        for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
        {
            for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
            {
                if (x == 0 && y == 0) continue;
                drawList?.AddText(position + new Vector2(x, y), outlineColor, text);
            }
        }

        // 原始文字
        drawList?.AddText(position, textColor, text);
    }
}

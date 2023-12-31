namespace OmenTools.ImGuiOm;

public static class Button
{
    public static bool ButtonIcon(FontAwesomeIcon icon, string tooltip = "", Vector2 size = default)
    {
        ImGui.PushID($"{icon.ToIconString()}_{tooltip}");
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button(icon.ToIconString(), size);
        ImGui.PopFont();

        if (ImGui.IsItemHovered()) Tooltip.TooltipHover(tooltip);
        ImGui.PopID();

        return result;
    }

    public static bool ButtonIconWithTextVertical(FontAwesomeIcon icon, string text)
    {
        ImGui.PushID($"{text}_{icon.ToIconString()}");
        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();
        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var spacing = 3f * ImGuiHelpers.GlobalScale;
        var buttonWidth = Math.Max(iconSize.X, textSize.X) + (padding * 2);
        var buttonHeight = iconSize.Y + textSize.Y + (padding * 2) + spacing;

        var result = ImGui.Button(string.Empty, new Vector2(buttonWidth, buttonHeight));

        var iconPos = new Vector2(
            cursorScreenPos.X + ((buttonWidth - iconSize.X) / 2),
            cursorScreenPos.Y + padding
        );

        var textPos = new Vector2(
            cursorScreenPos.X + ((buttonWidth - textSize.X) / 2),
            iconPos.Y + iconSize.Y + spacing
        );

        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }
}
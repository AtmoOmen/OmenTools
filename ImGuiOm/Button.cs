namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool ButtonIcon(string id, FontAwesomeIcon icon, string tooltip = "")
    {
        ImGui.PushID($"{id}_{icon}");
        var iconText = icon.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var vector = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var x = vector.X + ImGui.GetStyle().FramePadding.X * 2f;
        var frameHeight = ImGui.GetFrameHeight();
        var result = ImGui.Button(string.Empty, new Vector2(x, frameHeight));
        var pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), iconText);
        ImGui.PopFont();
        ImGui.PopID();

        if (!tooltip.IsNullOrEmpty()) TooltipHover(tooltip);

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

        var iconPos = new Vector2(cursorScreenPos.X + ((buttonWidth - iconSize.X) / 2), cursorScreenPos.Y + padding);
        var textPos = new Vector2(cursorScreenPos.X + ((buttonWidth - textSize.X) / 2), iconPos.Y + iconSize.Y + spacing);

        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool ButtonIconSelectable(string id, FontAwesomeIcon icon)
    {
        ImGui.PushID(id);

        var style = ImGui.GetStyle();
        var padding = style.FramePadding.X;
        var colors = style.Colors;

        ImGui.PushFont(UiBuilder.IconFont);
        var size = new Vector2(ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize(icon.ToIconString()).Y + 2 * padding);
        ImGui.PopFont();

        ImGui.PushStyleColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive]);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered]);
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{id}", size);
        ImGui.PopFont();
        ImGui.PopStyleColor(3);

        ImGui.PopID();

        return result;
    }
}
namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool ButtonIcon(string id, FontAwesomeIcon icon, string tooltip = "")
    {
        ImGui.PushID($"{id}_{icon}");

        ImGui.PushFont(UiBuilder.IconFont);
        var iconText = icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();

        var padding = ImGui.GetStyle().FramePadding;

        var buttonSize = new Vector2(iconSize.X + padding.X * 2, ImGui.GetFrameHeight());
        var result = ImGui.Button(string.Empty, buttonSize);

        var textPos = ImGui.GetCursorScreenPos() + new Vector2(padding.X, padding.Y);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), iconText);
        ImGui.PopFont();

        ImGui.PopID();

        if (!tooltip.IsNullOrEmpty()) TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonIconWithTextVertical(FontAwesomeIcon icon, string text)
    {
        ImGui.PushID($"{text}_{icon.ToIconString()}");

        ImGui.PushFont(UiBuilder.IconFont);
        var iconText = icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();

        var textSize = ImGui.CalcTextSize(text);
        var padding = ImGui.GetStyle().FramePadding.X;
        var spacing = 3f * ImGuiHelpers.GlobalScale;
        var buttonWidth = Math.Max(iconSize.X, textSize.X) + padding * 2;
        var buttonHeight = iconSize.Y + textSize.Y + padding * 2 + spacing;
        var result = ImGui.Button(string.Empty, new Vector2(buttonWidth, buttonHeight));

        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var iconPos = cursorScreenPos + new Vector2((buttonWidth - iconSize.X) / 2, padding);
        var textPos = cursorScreenPos + new Vector2((buttonWidth - textSize.X) / 2, iconPos.Y + iconSize.Y + spacing);

        var drawList = ImGui.GetWindowDrawList();
        ImGui.PushFont(UiBuilder.IconFont);
        drawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), iconText);
        ImGui.PopFont();
        drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

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
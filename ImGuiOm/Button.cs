namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool ButtonIcon(string id, FontAwesomeIcon icon, string tooltip = "")
    {
        ImGui.PushID($"{id}_{icon}");

        var iconText = icon.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(iconText) + new Vector2();
        ImGui.PopFont();

        var style = ImGui.GetStyle();
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = style.FramePadding;
        var buttonWidth = iconSize.X + padding.X * 2;
        var buttonHeight = iconSize.Y + padding.Y * 2;
        var result = ImGui.Button(string.Empty, new Vector2(buttonWidth, buttonHeight));
        var iconPos = new Vector2(cursorPos.X + (buttonWidth - iconSize.X + padding.X / 3) / 2,
            cursorPos.Y + style.FramePadding.Y);

        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), iconText);
        ImGui.PopFont();

        ImGui.PopID();

        if (!tooltip.IsNullOrEmpty()) TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonCompact(string id, string text)
    {
        ImGui.PushID(id);
        var textSize = ImGui.CalcTextSize(text);

        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding;
        var buttonWidth = Math.Max(ImGui.GetContentRegionMax().X, textSize.X + padding.X * 2);
        var result = ImGui.Button(string.Empty, new Vector2(buttonWidth, textSize.Y + padding.Y * 2));

        ImGui.GetWindowDrawList()
            .AddText(new Vector2(cursorPos.X + (buttonWidth - textSize.X) / 2, cursorPos.Y + padding.Y),
                ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.SetWindowFontScale(1);
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
        var buttonWidth = Math.Max(iconSize.X, textSize.X) + padding * 2;
        var buttonHeight = iconSize.Y + textSize.Y + padding * 2 + spacing;

        var result = ImGui.Button(string.Empty, new Vector2(buttonWidth, buttonHeight));

        var iconPos = new Vector2(cursorScreenPos.X + (buttonWidth - iconSize.X) / 2, cursorScreenPos.Y + padding);
        var textPos = new Vector2(cursorScreenPos.X + (buttonWidth - textSize.X) / 2, iconPos.Y + iconSize.Y + spacing);

        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool ButtonIconWithTextVertical(FontAwesomeIcon icon, string text, Vector2 buttonSize)
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

        var result = ImGui.Button(string.Empty, buttonSize);

        var iconPos = new Vector2(
            cursorScreenPos.X + (buttonSize.X - iconSize.X) / 2,
            cursorScreenPos.Y + padding
        );

        var textPos = new Vector2(
            cursorScreenPos.X + (buttonSize.X - textSize.X) / 2,
            cursorScreenPos.Y + padding + iconSize.Y + spacing
        );

        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool ButtonIconSelectable(string id, FontAwesomeIcon icon, string tooltip = "")
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

        if (!tooltip.IsNullOrEmpty()) TooltipHover(tooltip);

        ImGui.PopID();

        return result;
    }

    public static bool ButtonSelectable(string text)
    {
        var style = ImGui.GetStyle();
        var padding = style.FramePadding;
        var colors = style.Colors;
        var textSize = ImGui.CalcTextSize(text);

        var size = new Vector2(Math.Max(ImGui.GetContentRegionAvail().X, textSize.X + 2 * padding.X),
            textSize.Y + 2 * padding.Y);

        ImGui.PushStyleColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive]);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered]);
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = ImGui.Button(text, size);
        ImGui.PopStyleColor(3);

        return result;
    }
}
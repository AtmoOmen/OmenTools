namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void Text(string text)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
        ImGui.Text(text);
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

    public static bool TextIcon(FontAwesomeIcon icon, string text)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();

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
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();

        var textPos = new Vector2(iconPos.X + iconSize.X + 2 * padding.X, cursorPos.Y + padding.Y);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }
}
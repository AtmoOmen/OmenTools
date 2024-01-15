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

    public static void TextIcon(FontAwesomeIcon icon, string text)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();

        ImGui.SameLine();
        Text(text);
    }
}
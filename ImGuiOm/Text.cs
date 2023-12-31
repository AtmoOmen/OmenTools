namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void TextCentered(string id, string text)
    {
        ImGui.PushID(id);
        CenterAlignFor(ImGui.CalcTextSize(text).X);
        ImGui.Text(text);
        ImGui.PopID();
    }
}
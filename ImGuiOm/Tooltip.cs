namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void TooltipHover(string text, float warpPos = 20f)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (!ImGui.IsItemHovered()) return;
        
        using var id = ImRaii.PushId($"TooltipHover_{text}_{warpPos}");
        
        using (ImRaii.Tooltip())
        using (ImRaii.TextWrapPos(ImGui.GetFontSize() * warpPos))
            ImGui.TextUnformatted(text);
    }
}

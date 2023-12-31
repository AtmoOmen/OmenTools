namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void TooltipHover(string text, float warpPos = 20f)
    {
        ImGui.PushID($"{text}_{warpPos}");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * warpPos);
            ImGui.Text(text);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
        ImGui.PopID();
    }
}
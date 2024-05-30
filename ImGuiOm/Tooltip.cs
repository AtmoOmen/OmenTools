namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void TooltipHover(string text, float warpPos = 20f)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

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

    public static void TooltipDisableHelp(List<KeyValuePair<bool, string>> conditions, string header = "Disabled for the following reasons")
    {
        var tooltips = (from condition in conditions where condition.Key select condition.Value).ToList();

        if (tooltips.Count <= 0) return;
        if (!ImGui.IsItemHovered()) return;

        ImGui.BeginTooltip();
        ImGui.TextColored(ImGuiColors.DalamudYellow, $"{header}:");
        for (var i = 0; i < tooltips.Count; i++)
            ImGui.Text($"{i + 1}. {tooltips[i]}");
        ImGui.EndTooltip();
    }
}
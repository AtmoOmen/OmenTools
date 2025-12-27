using Dalamud.Interface.Colors;

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
            ImGui.Text(text);
    }

    // TODO: 移除
    public static void TooltipDisableHelp(
        List<KeyValuePair<bool, string>> conditions,
        string                           header = "由于以下原因被禁用")
    {
        var tooltips = (from condition in conditions where condition.Key select condition.Value).ToList();

        if (tooltips.Count <= 0) return;
        if (!ImGui.IsItemHovered()) return;

        using (ImRaii.Tooltip())
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, $"{header}:");
            for (var i = 0; i < tooltips.Count; i++)
                ImGui.Text($"{i + 1}. {tooltips[i]}");
        }
    }
}

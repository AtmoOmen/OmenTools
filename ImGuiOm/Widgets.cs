using Dalamud.Interface;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool CompLabelLeft(string label, Func<bool> origInputFunc)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.SameLine();
        return origInputFunc();
    }

    public static bool CompLabelLeft(string label, float width, Func<bool> origInputFunc)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        return origInputFunc();
    }

    public static void CompLabelLeft(string label, Action origInputFunc)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.SameLine();
        origInputFunc();
    }

    public static void CompLabelLeft(string label, float width, Action origInputFunc)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        origInputFunc();
    }

    public static void HelpMarker(string tooltip, float warpPos = 20f, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool useStaticFont = false)
    {
        ImGui.SameLine();
        if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
        if (useStaticFont) ImGui.PopFont();
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * warpPos);
            ImGui.TextUnformatted(tooltip);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static void DisableZoneWithHelp(Action interfaceAction, List<KeyValuePair<bool, string>> conditions,
        string header = "Disabled for the following reasons")
    {
        var isNeedToDisable = conditions.Any(kvp => kvp.Key);

        ImGui.BeginGroup();
        ImGui.BeginDisabled(isNeedToDisable);
        interfaceAction.Invoke();
        ImGui.EndDisabled();
        ImGui.EndGroup();

        TooltipDisableHelp(conditions, header);
    }
}
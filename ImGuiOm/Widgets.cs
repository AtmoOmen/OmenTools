using Dalamud.Interface;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool CompLabelLeft(string label, Func<bool> origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(label);

            ImGui.SameLine();
            return origInputFunc();
        }
    }

    public static bool CompLabelLeft(string label, float width, Func<bool> origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(label);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(width);
            return origInputFunc();
        }
    }

    public static void CompLabelLeft(string label, Action origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(label);

            ImGui.SameLine();
            origInputFunc();
        }
    }

    public static void CompLabelLeft(string label, float width, Action origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(label);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(width);
            origInputFunc();
        }
    }

    public static void HelpMarker(string tooltip, float warpPos = 20f, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle, bool useStaticFont = false)
    {
        ImGui.SameLine();

        using (ImRaii.Group())
        {
            if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);

            var origPosY = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(origPosY + (ImGui.GetStyle().FramePadding.Y * 0.5f));
            ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
            ImGui.SetCursorPosY(origPosY);
            if (useStaticFont) ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                using (ImRaii.Tooltip())
                {
                    using (ImRaii.TextWrapPos(ImGui.GetFontSize() * warpPos))
                    {
                        ImGui.TextUnformatted(tooltip);
                    }
                }
            }
        }
    }

    public static void DisableZoneWithHelp(Action interfaceAction, List<KeyValuePair<bool, string>> conditions,
        string header = "由于以下原因被禁用")
    {
        var isNeedToDisable = conditions.Any(kvp => kvp.Key);

        using (ImRaii.Group())
        {
            using (ImRaii.Disabled(isNeedToDisable))
            {
                interfaceAction();
            }
        }

        TooltipDisableHelp(conditions, header);
    }
}

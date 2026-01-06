using Dalamud.Interface;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool CompLabelLeft(string label, Func<bool> origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);

            ImGui.SameLine();
            return origInputFunc();
        }
    }

    public static bool CompLabelLeft(string label, float width, Func<bool> origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);

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
            ImGui.TextUnformatted(label);

            ImGui.SameLine();
            origInputFunc();
        }
    }

    public static void CompLabelLeft(string label, float width, Action origInputFunc)
    {
        using (ImRaii.Group())
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);

            ImGui.SameLine();
            ImGui.SetNextItemWidth(width);
            origInputFunc();
        }
    }

    public static void HelpMarker(
        string tooltip,
        float warpPos = 20f,
        FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle,
        bool useStaticFont = false)
    {
        ImGui.SameLine();
        
        using var group = ImRaii.Group();
        
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
        {
            var origPosY = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(origPosY + (ImGui.GetStyle().FramePadding.Y * 0.5f));
            ImGui.TextDisabled(icon.ToIconString());
            ImGui.SetCursorPosY(origPosY);
        }
        
        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
            {
                using (ImRaii.TextWrapPos(ImGui.GetFontSize() * warpPos))
                    ImGui.TextUnformatted(tooltip);
            }
        }
    }
}

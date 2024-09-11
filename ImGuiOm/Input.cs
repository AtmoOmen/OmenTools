using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool InputUInt(string label, ref uint v)
    {
        var tempV = (int)v;
        var result = ImGui.InputInt(label, ref tempV);
        if (result)
        {
            tempV = Math.Max(0, tempV);
            v = (uint)tempV;
        }
        return result;
    }

    public static bool InputUInt(string label, ref uint v, int step)
    {
        var tempV = (int)v;
        var result = ImGui.InputInt(label, ref tempV, step);
        if (result)
        {
            tempV = Math.Max(0, tempV);
            v = (uint)tempV;
        }
        return result;
    }

    public static bool InputUInt(string label, ref uint v, int step, int step_fast)
    {
        var tempV = (int)v;
        var result = ImGui.InputInt(label, ref tempV, step, step_fast);
        if (result)
        {
            tempV = Math.Max(0, tempV);
            v = (uint)tempV;
        }
        return result;
    }

    public static bool InputUInt(string label, ref uint v, int step, int step_fast, ImGuiInputTextFlags flags)
    {
        var tempV = (int)v;
        var result = ImGui.InputInt(label, ref tempV, step, step_fast, flags);
        if (result)
        {
            tempV = Math.Max(0, tempV);
            v = (uint)tempV;
        }
        return result;
    }
}
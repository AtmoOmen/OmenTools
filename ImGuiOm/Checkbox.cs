using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool CheckboxColored(
        string   label,
        ref bool selected,
        Vector4? enabledColor  = null,
        Vector4? disabledColor = null)
    {
        using var color0 = ImRaii.PushColor(ImGuiCol.Text, enabledColor  ?? KnownColor.Yellow.ToVector4(), selected);
        using var color1 = ImRaii.PushColor(ImGuiCol.Text, disabledColor ?? KnownColor.White.ToVector4(),  !selected);
        return ImGui.Checkbox(label, ref selected);
    }
}

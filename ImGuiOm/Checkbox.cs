using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool CheckboxColored
    (
        string   label,
        ref bool selected,
        uint?    enabledColor  = null,
        uint?    disabledColor = null
    )
    {
        using var color0 = ImRaii.PushColor(ImGuiCol.Text, enabledColor  ?? KnownColor.Yellow.ToUInt(), selected);
        using var color1 = ImRaii.PushColor(ImGuiCol.Text, disabledColor ?? KnownColor.White.ToUInt(),  !selected);
        return ImGui.Checkbox(label, ref selected);
    }

    public static bool CheckboxColored
    (
        string   label,
        ref bool selected,
        Vector4? enabledColor,
        Vector4? disabledColor = null
    ) =>
        CheckboxColored
        (
            label,
            ref selected,
            enabledColor?.ToUInt(),
            disabledColor?.ToUInt()
        );
}

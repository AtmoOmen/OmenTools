namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>
    ///     Draw a checkbox with a colored label that indicates whether the checkbox is checked.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="selected"></param>
    /// <param name="enabledColor">If left null, the color will be ImGuiColors.DalamudYellow</param>
    /// <param name="disabledColor">If left null, the color will be ImGuiColors.DalamudWhite</param>
    /// <returns></returns>
    public static bool CheckboxColored(string label, ref bool selected, Vector4? enabledColor = null,
        Vector4? disabledColor = null)
    {
        var color1 = enabledColor ?? ImGuiColors.DalamudYellow;
        var color2 = disabledColor ?? ImGuiColors.DalamudWhite;
        ImGui.PushStyleColor(ImGuiCol.Text, selected ? color1 : color2);
        var result = ImGui.Checkbox(label, ref selected);
        ImGui.PopStyleColor();

        return result;
    }
}
using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>根据选中状态使用不同的文本颜色绘制复选框。</summary>
    /// <param name="label">复选框标签。</param>
    /// <param name="selected">当前状态，点击后更新。</param>
    /// <param name="enabledColor">选中时的打包文本颜色；默认使用黄色。</param>
    /// <param name="disabledColor">未选中时的打包文本颜色；默认使用白色。</param>
    /// <returns>状态变化时返回真。</returns>
    public static bool CheckboxColored
    (
        string   label,
        ref bool selected,
        uint?    enabledColor  = null,
        uint?    disabledColor = null
    )
    {
        var color = selected ?
                        enabledColor  ?? KnownColor.Yellow.ToUInt() :
                        disabledColor ?? KnownColor.White.ToUInt();
        using var colorScope = ImRaii.PushColor(ImGuiCol.Text, color);
        return ImGui.Checkbox(label, ref selected);
    }

    /// <summary>使用向量颜色按选中状态绘制复选框。</summary>
    /// <param name="label">复选框标签。</param>
    /// <param name="selected">当前状态，点击后更新。</param>
    /// <param name="enabledColor">选中时的文本颜色。</param>
    /// <param name="disabledColor">未选中时的文本颜色。</param>
    /// <returns>状态变化时返回真。</returns>
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

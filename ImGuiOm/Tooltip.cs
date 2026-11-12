namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>当最近提交的控件被悬停时显示自动换行的提示。</summary>
    /// <param name="text">提示文本；为空白时不显示。</param>
    /// <param name="warpPos">换行位置，相对于当前字体大小与界面缩放计算。</param>
    public static void TooltipHover(string text, float warpPos = 20f)
    {
        if (string.IsNullOrWhiteSpace(text) || !ImGui.IsItemHovered())
            return;

        using (ImRaii.Tooltip())
        using (ImRaii.TextWrapPos(ImGui.GetFontSize() * GlobalUIScale * warpPos))
            ImGui.TextUnformatted(text);
    }
}

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>在输入控件左侧绘制对齐到控件基线的标签。</summary>
    /// <param name="label">显示标签。</param>
    /// <param name="origInputFunc">绘制输入控件并返回状态变化的委托。</param>
    /// <returns>输入控件委托的返回值。</returns>
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

    /// <summary>在指定宽度的输入控件左侧绘制标签。</summary>
    /// <param name="label">显示标签。</param>
    /// <param name="width">输入控件宽度。</param>
    /// <param name="origInputFunc">绘制输入控件并返回状态变化的委托。</param>
    /// <returns>输入控件委托的返回值。</returns>
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

    /// <summary>在自定义输入控件左侧绘制对齐到控件基线的标签。</summary>
    /// <param name="label">显示标签。</param>
    /// <param name="origInputFunc">绘制输入控件的委托。</param>
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

    /// <summary>设置输入控件宽度，并在其左侧绘制标签。</summary>
    /// <param name="label">显示标签。</param>
    /// <param name="width">输入控件宽度。</param>
    /// <param name="origInputFunc">绘制输入控件的委托。</param>
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

    /// <summary>在同一行绘制帮助图标，并在悬停时显示提示。</summary>
    /// <param name="tooltip">提示文本。</param>
    /// <param name="warpPos">提示的换行位置，相对于当前字体大小与界面缩放计算。</param>
    /// <param name="icon">帮助图标。</param>
    public static void HelpMarker
    (
        string          tooltip,
        float           warpPos = 40f,
        FontAwesomeIcon icon    = FontAwesomeIcon.InfoCircle
    )
    {
        ImGui.SameLine();

        using var group = ImRaii.Group();

        var text    = icon.ToIconString();
        var metrics = TextMetrics.Measure(text);
        metrics.Draw(text, ImGui.GetColorU32(ImGuiCol.TextDisabled), metrics.Size with { Y = ImGui.GetFrameHeight() });
        TooltipHover(tooltip, warpPos);
    }
}

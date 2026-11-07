using System.Globalization;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>
    ///     在同一行绘制小时、分钟、秒三个输入框，并使用指定区域文化的时间分隔符。
    /// </summary>
    /// <param name="id">用于区分控件的唯一标识，不显示在界面上。</param>
    /// <param name="time">当前时间；输入变化时更新为有效的二十四小时制时间。</param>
    /// <param name="culture">时间格式使用的区域文化；未指定时使用当前系统区域文化。</param>
    /// <returns>任一时间分量发生变化时返回 <see langword="true" />。</returns>
    public static bool InputTime(string id, ref TimeOnly time, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        using var scope = ImRaii.PushId(id);
        using var group = ImRaii.Group();

        var hours   = time.Hour;
        var minutes = time.Minute;
        var seconds = time.Second;
        var width   = ImGui.CalcTextSize("00").X + (ImGui.GetStyle().FramePadding.X * 2);

        ImGui.SetNextItemWidth(width);
        var changed = ImGui.InputInt("##Hour", ref hours);

        ImGui.SameLine();
        ImGui.TextUnformatted(culture.DateTimeFormat.TimeSeparator);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        changed |= ImGui.InputInt("##Minute", ref minutes);

        ImGui.SameLine();
        ImGui.TextUnformatted(culture.DateTimeFormat.TimeSeparator);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        changed |= ImGui.InputInt("##Second", ref seconds);

        if (changed)
            time = new TimeOnly(Math.Clamp(hours, 0, 23), Math.Clamp(minutes, 0, 59), Math.Clamp(seconds, 0, 59));

        return changed;
    }
}

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     单个 IndicatorEntry 在 Update 阶段的预计算结果, 包含已解析的绘制目标数组
/// </summary>
internal sealed class CachedEntryState
{
    internal CachedEntryState(Action<ZoneIndicatorDrawContext>? onDraw, CachedDrawTarget[] targets)
    {
        OnDraw  = onDraw;
        Targets = targets;
    }

    /// <summary>自定义绘制委托, null 表示仅绘制文字</summary>
    public readonly Action<ZoneIndicatorDrawContext>? OnDraw;

    /// <summary>经距离剔除与遮挡剔除后的绘制目标</summary>
    public readonly CachedDrawTarget[] Targets;
}

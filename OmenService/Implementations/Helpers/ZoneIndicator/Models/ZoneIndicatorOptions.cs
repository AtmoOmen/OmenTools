namespace OmenTools.OmenService.ZoneIndicator;

public class ZoneIndicatorOptions
{
    /// <summary>渲染半径范围 (yalm), null 表示不做距离限制</summary>
    public uint? RenderRadius { get; init; }

    /// <summary>目标地点被地形遮挡时是否不渲染, 默认 false</summary>
    public bool HiddenWhenBlocked { get; init; }

    /// <summary>不绘制文字标签背景, 默认 false</summary>
    public bool HideTextLabel { get; init; }

    /// <summary>目标周围的包围形状, null 表示不绘制</summary>
    public ZoneIndicatorSurrounding? Surrounding { get; init; }
}

public sealed class ZoneIndicatorOptions<T> : ZoneIndicatorOptions
{
    /// <summary>从 T 提取显示文字 / 图片, null 表示不绘制文字</summary>
    public Func<T, ZoneIndicatorText>? TextGetter { get; init; }

    /// <summary>自定义绘制, 每个 T 实例独立回调, null 表示仅绘制文字与包围形状</summary>
    public Action<ZoneIndicatorDrawContext<T>>? CustomDraw { get; init; }
}

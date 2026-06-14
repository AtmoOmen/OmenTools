namespace OmenTools.OmenService.ImGuiZoneObject;

public sealed class ZoneIndicatorOptions
{
    internal static readonly ZoneIndicatorOptions Default = new();

    /// <summary>
    ///     渲染半径范围 (yalm), 目标超出该距离时不渲染, 默认 100
    /// </summary>
    public uint RenderRadius { get; init; } = 100;

    /// <summary>
    ///     目标地点被地形遮挡时是否不渲染, 默认 false
    /// </summary>
    public bool HiddenWhenBlocked { get; init; }

    /// <summary>
    ///     目标周围的包围形状, null 表示不绘制
    /// </summary>
    public ZoneIndicatorSurrounding? Surrounding { get; init; }
}

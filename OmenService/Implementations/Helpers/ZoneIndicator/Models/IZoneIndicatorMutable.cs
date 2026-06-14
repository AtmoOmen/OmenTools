namespace OmenTools.OmenService.ZoneIndicator;

public interface IZoneIndicatorMutable
{
    /// <summary>渲染半径范围 (yalm), null 表示不做距离限制</summary>
    uint? RenderRadius { get; set; }

    /// <summary>目标地点被地形遮挡时是否不渲染</summary>
    bool HiddenWhenBlocked { get; set; }

    /// <summary>目标周围的包围形状, null 表示不绘制</summary>
    ZoneIndicatorSurrounding? Surrounding { get; set; }
}

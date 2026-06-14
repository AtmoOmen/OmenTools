using System.Numerics;

namespace OmenTools.OmenService.ZoneIndicator;

public readonly ref struct ZoneIndicatorDrawContext
{
    internal ZoneIndicatorDrawContext
    (
        Vector3       worldPosition,
        Vector2       screenPosition,
        bool          isOnScreen,
        float         distance,
        ImDrawListPtr drawList,
        Vector2       textSize
    )
    {
        WorldPosition  = worldPosition;
        ScreenPosition = screenPosition;
        IsOnScreen     = isOnScreen;
        Distance       = distance;
        DrawList       = drawList;
        TextSize       = textSize;
    }

    /// <summary>
    ///     目标世界坐标
    /// </summary>
    public Vector3 WorldPosition { get; }

    /// <summary>
    ///     目标屏幕坐标, 仅 <see cref="IsOnScreen" /> 为 true 时有效
    /// </summary>
    public Vector2 ScreenPosition { get; }

    /// <summary>
    ///     目标是否位于屏幕内
    /// </summary>
    public bool IsOnScreen { get; }

    /// <summary>
    ///     目标到玩家的距离 (yalm)
    /// </summary>
    public float Distance { get; }

    /// <summary>
    ///     可直接使用的前景绘制列表
    /// </summary>
    public ImDrawListPtr DrawList { get; }

    /// <summary>
    ///     上一帧缓存的文字渲染尺寸, 首次绘制使用默认值 50×20
    /// </summary>
    public Vector2 TextSize { get; }
}

using System.Numerics;

namespace OmenTools.OmenService.ZoneIndicator;

public readonly struct ZoneIndicatorDrawContext
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

    public Vector3 WorldPosition  { get; }
    public Vector2 ScreenPosition { get; }
    public bool    IsOnScreen     { get; }
    public float   Distance       { get; }

    /// <summary>可直接使用的前景绘制列表</summary>
    public ImDrawListPtr DrawList { get; }

    /// <summary>上一帧缓存的文字渲染尺寸, 首次绘制使用默认值 50×20</summary>
    public Vector2 TextSize { get; }
}

public readonly struct ZoneIndicatorDrawContext<T>
{
    internal ZoneIndicatorDrawContext(ZoneIndicatorDrawContext baseCtx, T source)
    {
        WorldPosition  = baseCtx.WorldPosition;
        ScreenPosition = baseCtx.ScreenPosition;
        IsOnScreen     = baseCtx.IsOnScreen;
        Distance       = baseCtx.Distance;
        DrawList       = baseCtx.DrawList;
        TextSize       = baseCtx.TextSize;
        Source         = source;
    }

    public Vector3       WorldPosition  { get; }
    public Vector2       ScreenPosition { get; }
    public bool          IsOnScreen     { get; }
    public float         Distance       { get; }
    public ImDrawListPtr DrawList       { get; }
    public Vector2       TextSize       { get; }
    public T             Source         { get; }
}

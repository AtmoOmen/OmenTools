using System.Numerics;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     Update 阶段预解析的单个绘制目标, 供 Draw 热路径直接消费
/// </summary>
internal readonly struct CachedDrawTarget
{
    internal CachedDrawTarget(Vector3 worldPosition, float distance, ZoneIndicatorText? text)
    {
        WorldPosition = worldPosition;
        Distance      = distance;
        Text          = text;
    }

    /// <summary>目标世界坐标</summary>
    public readonly Vector3 WorldPosition;

    /// <summary>目标到玩家的距离 (yalm), 在 Update 阶段计算</summary>
    public readonly float Distance;

    /// <summary>已解析的文字参数, null 表示不绘制文字</summary>
    public readonly ZoneIndicatorText? Text;
}

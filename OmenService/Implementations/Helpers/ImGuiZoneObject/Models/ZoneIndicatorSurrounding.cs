using System.Numerics;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     目标周围的包围形状参数, 在世界 XZ 水平面上生成形状顶点, 投影到屏幕后以 polyline 绘制
/// </summary>
public sealed class ZoneIndicatorSurrounding
{
    /// <summary>形状类型</summary>
    public Shape Type { get; init; } = Shape.Circle;

    /// <summary>
    ///     圆形为半径, 正方形为半边长, 三角形为外接圆半径
    /// </summary>
    public float Radius { get; init; }

    /// <summary>描边颜色</summary>
    public Vector4 Color { get; init; } = Vector4.One;

    /// <summary>描边粗细 (像素)</summary>
    public float Thickness { get; init; } = 1f;
    
    /// <summary>
    ///     包围形状类型
    /// </summary>
    public enum Shape
    {
        /// <summary>圆形</summary>
        Circle,

        /// <summary>正方形</summary>
        Square,

        /// <summary>等边三角形, 顶点朝上</summary>
        Triangle
    }
}

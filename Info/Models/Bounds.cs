using System.Numerics;

namespace OmenTools.Info.Models;

/// <summary>
///     表示由两个对角坐标确定的二维边界。
/// </summary>
public class Bounds
{
    /// <summary>
    ///     获取或设置左上角坐标。
    /// </summary>
    public required Vector2 TopLeft { get; set; }

    /// <summary>
    ///     获取或设置右下角坐标。
    /// </summary>
    public required Vector2 BottomRight { get; set; }

    /// <summary>
    ///     获取上边缘的纵坐标。
    /// </summary>
    public float Top => TopLeft.Y;

    /// <summary>
    ///     获取左边缘的横坐标。
    /// </summary>
    public float Left => TopLeft.X;

    /// <summary>
    ///     获取下边缘的纵坐标。
    /// </summary>
    public float Bottom => BottomRight.Y;

    /// <summary>
    ///     获取右边缘的横坐标。
    /// </summary>
    public float Right => BottomRight.X;

    /// <summary>
    ///     获取边界宽度。
    /// </summary>
    public float Width => Math.Abs(BottomRight.X - TopLeft.X);

    /// <summary>
    ///     获取边界高度。
    /// </summary>
    public float Height => Math.Abs(BottomRight.Y - TopLeft.Y);

    /// <summary>
    ///     获取边界宽度和高度。
    /// </summary>
    public Vector2 Size => new(Width, Height);

    /// <summary>
    ///     获取中心点的横坐标。
    /// </summary>
    public float CenterX => (TopLeft.X + BottomRight.X) / 2.0f;

    /// <summary>
    ///     获取中心点的纵坐标。
    /// </summary>
    public float CenterY => (TopLeft.Y + BottomRight.Y) / 2.0f;

    /// <summary>
    ///     获取中心点坐标。
    /// </summary>
    public Vector2 Center => new(CenterX, CenterY);

    /// <summary>
    ///     将边界的两个对角坐标转换为文本。
    /// </summary>
    /// <returns>左上角与右下角坐标的文本表示。</returns>
    public override string ToString() => $"{TopLeft}, {BottomRight}";
}

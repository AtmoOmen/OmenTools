namespace OmenTools.Info.Game.Enums;

/// <summary>
///     原生界面节点的绘制、交互与变换标志。
/// </summary>
[Flags]
public enum DrawFlags : uint
{
    /// <summary>
    ///     不设置任何标志。
    /// </summary>
    None = 0,

    /// <summary>
    ///     在下次更新时重新计算节点状态。
    /// </summary>
    IsDirty = 0x1,

    /// <summary>
    ///     节点正在播放动画。
    /// </summary>
    IsAnimating = 0x2,

    /// <summary>
    ///     计算节点变换。
    /// </summary>
    CalculateTransformation = 0x4,

    /// <summary>
    ///     禁止向上连续导航。
    /// </summary>
    DisableRapidUp = 0x10,

    /// <summary>
    ///     禁止向下连续导航。
    /// </summary>
    DisableRapidDown = 0x20,

    /// <summary>
    ///     禁止向左连续导航。
    /// </summary>
    DisableRapidLeft = 0x40,

    /// <summary>
    ///     禁止向右连续导航。
    /// </summary>
    DisableRapidRight = 0x80,

    /// <summary>
    ///     禁用时间线标签。
    /// </summary>
    DisableTimelineLabel = 0x100,

    /// <summary>
    ///     鼠标悬停时显示手形光标。
    /// </summary>
    ClickableCursor = 0x100000,

    /// <summary>
    ///     忽略节点树中的位置，将节点绘制在其他节点上方。
    /// </summary>
    RenderOnTop = 0x200000,

    /// <summary>
    ///     鼠标悬停时显示文本输入光标。
    /// </summary>
    TextInputCursor = 0x400000,

    /// <summary>
    ///     根据节点宽高计算椭圆碰撞区域。
    /// </summary>
    UseEllipticalCollision = 0x800000,

    /// <summary>
    ///     根据节点实际尺寸、缩放与旋转变换计算碰撞区域。
    /// </summary>
    UseTransformedCollision = 0x1000000
}

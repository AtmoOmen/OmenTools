using System.Numerics;
using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     标记文字参数, 用于按物体或位置动态生成文字
/// </summary>
public sealed class ZoneIndicatorText
{
    /// <summary>
    ///     显示文字, null 表示不绘制文字
    /// </summary>
    public ReadOnlySeString? Text { get; init; }

    /// <summary>
    ///     文字颜色, null 回退默认白色
    /// </summary>
    public Vector4? TextColor { get; init; }

    /// <summary>
    ///     文字缩放, null 回退 1.0
    /// </summary>
    public float? TextScale { get; init; }

    /// <summary>
    ///     文字屏幕坐标偏移 (像素), null 回退零向量
    /// </summary>
    public Vector3? TextOffset { get; init; }
}

using System.Numerics;
using Dalamud.Interface.Textures;
using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService.ImGuiZoneObject;

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
    ///     文字屏幕坐标偏移, null 回退零向量
    /// </summary>
    public Vector3? TextOffset { get; init; }

    /// <summary>
    ///     文字前绘制的图片, null 表示不绘制图片
    /// </summary>
    public TextImage? Image { get; init; }

    /// <summary>
    ///     标记文字附带的图片参数
    /// </summary>
    public sealed class TextImage
    {
        /// <summary>图片纹理</summary>
        public required ISharedImmediateTexture Texture { get; init; }

        /// <summary>图片尺寸</summary>
        public required Func<Vector2> Size { get; init; }
    }
}

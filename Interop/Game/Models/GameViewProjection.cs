using System.Numerics;
using System.Runtime.CompilerServices;
using OmenTools.Interop.Game.Helpers;

namespace OmenTools.Interop.Game.Models;

/// <summary>
///     当前活动相机与主视口的不可变投影快照，可在一次绘制中投影多个世界坐标。
/// </summary>
/// <remarks>
///     通过 <see cref="GameViewHelper.TryGetProjection" /> 获取。快照仅保存矩阵和视口数值，
///     不持有客户端对象；相机或视口变化后应重新获取。
/// </remarks>
public readonly record struct GameViewProjection
{
    internal readonly Vector2 ViewportOrigin;
    internal readonly Vector2 ViewportHalfSize;

    private readonly Matrix4x4 viewProjection;

    internal GameViewProjection
    (
        Matrix4x4 view,
        Matrix4x4 projection,
        Vector2   viewportOrigin,
        Vector2   viewportHalfSize
    )
    {
        ViewportOrigin   = viewportOrigin;
        ViewportHalfSize = viewportHalfSize;
        viewProjection   = view * projection;
    }

    /// <summary>
    ///     使用快照将世界坐标转换为屏幕坐标，并判断该点是否处于主视口范围内。
    /// </summary>
    /// <param name="worldPos">需要投影的游戏世界坐标。</param>
    /// <param name="screenPos">投影后的桌面屏幕坐标，已包含主视口的位置偏移。</param>
    /// <returns>该点位于相机前方且处于主视口范围内时返回 <see langword="true" />。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WorldToScreen
    (
        Vector3     worldPos,
        out Vector2 screenPos
    ) =>
        WorldToScreen(worldPos, out screenPos, out var inView) && inView;

    /// <summary>
    ///     使用快照将世界坐标转换为屏幕坐标，分别返回相机前方判断和主视口范围判断。
    /// </summary>
    /// <param name="worldPos">需要投影的游戏世界坐标。</param>
    /// <param name="screenPos">投影后的桌面屏幕坐标，已包含主视口的位置偏移。</param>
    /// <param name="inView">该点位于相机前方且处于主视口范围内时为 <see langword="true" />。</param>
    /// <returns>该点能够投影到相机前方时返回 <see langword="true" />，不要求位于视口内。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WorldToScreen
    (
        Vector3     worldPos,
        out Vector2 screenPos,
        out bool    inView
    )
    {
        var clip    = Vector4.Transform(new Vector4(worldPos, 1f), viewProjection);
        var inFront = clip.W > 0f;

        if (MathF.Abs(clip.W) < float.Epsilon)
        {
            screenPos = default;
            inView    = false;
            return false;
        }

        var normalized = new Vector2(clip.X, -clip.Y) * MathF.Abs(1f / clip.W);
        screenPos = ((normalized + Vector2.One)       * ViewportHalfSize) + ViewportOrigin;
        inView    = inFront && normalized is { X: >= -1f and <= 1f, Y: >= -1f and <= 1f };
        return inFront;
    }
}

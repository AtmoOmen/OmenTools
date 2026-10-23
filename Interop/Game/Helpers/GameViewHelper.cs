using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using OmenTools.Interop.Game.Models;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     使用当前活动相机在游戏世界坐标与主视口屏幕坐标之间进行转换。
/// </summary>
/// <remarks>
///     应在游戏主线程或界面绘制回调中调用。矩阵或视口发生变化时立即刷新缓存，
///     不依赖目标系统的投影缓存或帧号。
/// </remarks>
public static class GameViewHelper
{
    private static Matrix4x4          CameraView;
    private static Matrix4x4          CameraProjection;
    private static GameViewProjection Projection;

    /// <summary>
    ///     获取当前活动相机和主视口的投影快照，供同一批世界坐标重复使用。
    /// </summary>
    /// <param name="projection">成功时返回当前投影快照，失败时返回默认值。</param>
    /// <returns>活动相机、渲染相机和图形设备均可用时返回 <see langword="true" />。</returns>
    /// <remarks>快照仅保存数值。应在每次绘制开始时获取，以使用当前相机状态。</remarks>
    public static unsafe bool TryGetProjection
    (
        out GameViewProjection projection
    )
    {
        var control = Control.Instance();
        var device  = Device.Instance();
        var camera  = control->CameraManager.GetActiveCamera();

        if (camera == null || camera->CameraBase.SceneCamera.RenderCamera == null || device == null)
        {
            projection = default;
            return false;
        }

        Matrix4x4 view             = camera->CameraBase.SceneCamera.ViewMatrix;
        Matrix4x4 projectionMatrix = camera->CameraBase.SceneCamera.RenderCamera->ProjectionMatrix;
        var       viewportOrigin   = ImGuiHelpers.MainViewport.Pos;
        var       viewportHalfSize = new Vector2(device->Width * 0.5f, device->Height * 0.5f);

        if (CameraView                  != view             ||
            CameraProjection            != projectionMatrix ||
            Projection.ViewportOrigin   != viewportOrigin   ||
            Projection.ViewportHalfSize != viewportHalfSize)
        {
            CameraView       = view;
            CameraProjection = projectionMatrix;
            Projection       = new(view, projectionMatrix, viewportOrigin, viewportHalfSize);
        }

        projection = Projection;
        return true;
    }

    /// <summary>
    ///     将世界坐标转换为屏幕坐标，并判断该点是否位于相机前方的主视口范围内。
    /// </summary>
    /// <param name="worldPos">需要投影的游戏世界坐标。</param>
    /// <param name="screenPos">投影后的桌面屏幕坐标，已包含主视口的位置偏移。</param>
    /// <returns>投影成功且位于主视口范围内时返回 <see langword="true" />。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WorldToScreen
    (
        Vector3     worldPos,
        out Vector2 screenPos
    ) =>
        WorldToScreen(worldPos, out screenPos, out var inView) && inView;

    /// <summary>
    ///     将世界坐标转换为屏幕坐标，分别返回相机前方判断和主视口范围判断。
    /// </summary>
    /// <param name="worldPos">需要投影的游戏世界坐标。</param>
    /// <param name="screenPos">投影后的桌面屏幕坐标，已包含主视口的位置偏移。</param>
    /// <param name="inView">该点位于相机前方且处于主视口范围内时为 <see langword="true" />。</param>
    /// <returns>相机可用且该点能够投影到相机前方时返回 <see langword="true" />，不要求位于视口内。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WorldToScreen
    (
        Vector3     worldPos,
        out Vector2 screenPos,
        out bool    inView
    )
    {
        if (TryGetProjection(out var projection))
            return projection.WorldToScreen(worldPos, out screenPos, out inView);

        screenPos = default;
        inView    = false;
        return false;
    }

    /// <summary>
    ///     从主视口屏幕坐标沿当前活动相机的原生射线查询场景，返回最近的世界坐标命中点。
    /// </summary>
    /// <param name="screenPos">包含主视口位置偏移的桌面屏幕坐标，与 <see cref="WorldToScreen(Vector3, out Vector2)" /> 的输出一致。</param>
    /// <param name="worldPos">成功时为场景碰撞命中点，失败时为默认值。</param>
    /// <param name="rayDistance">从原生射线起点开始计算的最大检测距离，必须为有限正数，默认 100000。</param>
    /// <returns>屏幕坐标位于主视口内、相机可用且射线命中场景时返回 <see langword="true" />。</returns>
    /// <remarks>
    ///     使用游戏原生屏幕射线与材质过滤规则，适合鼠标拾取地形和场景物体。
    ///     不命中场景的屏幕坐标无法唯一确定世界位置，此时返回 <see langword="false" />。
    ///     仅在调用时执行碰撞查询，不影响正向投影的缓存和计算开销。
    /// </remarks>
    public static unsafe bool ScreenToWorld
    (
        Vector2     screenPos,
        out Vector3 worldPos,
        float       rayDistance = 100000f
    )
    {
        worldPos = default;

        if (!float.IsFinite(screenPos.X) ||
            !float.IsFinite(screenPos.Y) ||
            !float.IsFinite(rayDistance) ||
            rayDistance <= 0f)
            return false;

        var device = Device.Instance();
        if (device == null)
            return false;

        var localPosition = screenPos - ImGuiHelpers.MainViewport.Pos;
        if (localPosition.X < 0f            ||
            localPosition.X > device->Width ||
            localPosition.Y < 0f            ||
            localPosition.Y > device->Height)
            return false;

        var camera = Control.Instance()->CameraManager.GetActiveCamera();
        if (camera == null || camera->CameraBase.SceneCamera.RenderCamera == null)
            return false;

        var ray = camera->CameraBase.SceneCamera.ScreenPointToRay(localPosition);
        if (!BGCollisionModule.RaycastMaterialFilter(ray.Origin, ray.Direction, out var hit, rayDistance))
            return false;

        worldPos = hit.Point;
        return true;
    }
}

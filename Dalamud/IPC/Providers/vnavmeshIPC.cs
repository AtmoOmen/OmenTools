using System.Numerics;
using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

// ReSharper disable once InconsistentNaming
public static class vnavmeshIPC
{
    public const string INTERNAL_NAME = "vnavmesh";

    [IPCSubscriber("vnavmesh.Nav.IsReady", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavIsReady;

    [IPCSubscriber("vnavmesh.Nav.BuildProgress", DefaultValue = "0")]
    private static IPCSubscriber<float>? NavBuildProgress;

    [IPCSubscriber("vnavmesh.Nav.Reload")]
    private static IPCSubscriber<bool>? NavReload;

    [IPCSubscriber("vnavmesh.Nav.Rebuild")]
    private static IPCSubscriber<bool>? NavRebuild;

    [IPCSubscriber("vnavmesh.Nav.Pathfind")]
    private static IPCSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>>? NavPathfind;

    [IPCSubscriber("vnavmesh.Nav.PathfindCancelable")]
    private static IPCSubscriber<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>>? NavPathfindCancelable;

    [IPCSubscriber("vnavmesh.Nav.IsAutoLoad", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavIsAutoLoad;

    [IPCSubscriber("vnavmesh.Nav.SetAutoLoad")]
    private static IPCSubscriber<bool, object>? NavSetAutoLoad;

    [IPCSubscriber("vnavmesh.Nav.PathfindNumQueued", DefaultValue = "0")]
    private static IPCSubscriber<int>? NavPathfindNumQueued;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmap")]
    private static IPCSubscriber<Vector3, string, float, bool>? NavBuildBitmap;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmapBounded")]
    private static IPCSubscriber<Vector3, string, float, Vector3, Vector3, bool>? NavBuildBitmapBounded;

    [IPCSubscriber("vnavmesh.Query.Mesh.NearestPoint")]
    private static IPCSubscriber<Vector3, float, float, Vector3?>? QueryMeshNearestPoint;

    [IPCSubscriber("vnavmesh.Query.Mesh.PointOnFloor")]
    private static IPCSubscriber<Vector3, bool, float, Vector3?>? QueryPointOnFloor;

    [IPCSubscriber("vnavmesh.Path.MoveTo")]
    private static IPCSubscriber<List<Vector3>, bool, object>? PathMoveTo;

    [IPCSubscriber("vnavmesh.Path.Stop")]
    private static IPCSubscriber<object>? PathStop;

    [IPCSubscriber("vnavmesh.Path.IsRunning", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathIsRunning;

    [IPCSubscriber("vnavmesh.Path.NumWaypoints", DefaultValue = "0")]
    private static IPCSubscriber<int>? PathNumWaypoints;

    [IPCSubscriber("vnavmesh.Path.ListWaypoints")]
    private static IPCSubscriber<List<Vector3>>? PathListWaypoints;

    [IPCSubscriber("vnavmesh.Path.GetMovementAllowed", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathGetMovementAllowed;

    [IPCSubscriber("vnavmesh.Path.SetMovementAllowed")]
    private static IPCSubscriber<bool, object>? PathSetMovementAllowed;

    [IPCSubscriber("vnavmesh.Path.GetAlignCamera", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathGetAlignCamera;

    [IPCSubscriber("vnavmesh.Path.SetAlignCamera")]
    private static IPCSubscriber<bool, object>? PathSetAlignCamera;

    [IPCSubscriber("vnavmesh.Path.GetTolerance", DefaultValue = "0")]
    private static IPCSubscriber<float>? PathGetTolerance;

    [IPCSubscriber("vnavmesh.Path.SetTolerance")]
    private static IPCSubscriber<float, object>? PathSetTolerance;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindAndMoveTo")]
    private static IPCSubscriber<Vector3, bool, bool>? SimpleMovePathfindAndMoveTo;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindAndMoveCloseTo")]
    private static IPCSubscriber<Vector3, bool, float, bool>? PathfindAndMoveToCloseTo;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindInProgress", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathfindInProgress;

    [IPCSubscriber("vnavmesh.Nav.PathfindInProgress", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavPathfindInProgress;

    [IPCSubscriber("vnavmesh.Nav.PathfindCancelAll")]
    private static IPCSubscriber<object>? PathfindCancelAll;

    [IPCSubscriber("vnavmesh.Window.IsOpen", DefaultValue = "false")]
    private static IPCSubscriber<bool>? WindowIsOpen;

    [IPCSubscriber("vnavmesh.Window.SetOpen")]
    private static IPCSubscriber<bool, object>? WindowSetOpen;

    [IPCSubscriber("vnavmesh.DTR.IsShown", DefaultValue = "false")]
    private static IPCSubscriber<bool>? DTRIsShown;

    [IPCSubscriber("vnavmesh.DTR.SetShown")]
    private static IPCSubscriber<bool, object>? DTRSetShown;

    [IPCSubscriber("vnavmesh.Path.GetDistance", DefaultValue = "0")]
    private static IPCSubscriber<float>? PathGetDistance;

    /// <summary>
    ///     检查剩余路径距离
    /// </summary>
    public static float GetPathLeftDistance() =>
        PathGetDistance ?? 0f;

    /// <summary>
    ///     检查导航网格是否准备就绪
    /// </summary>
    /// <returns></returns>
    public static bool GetIsNavReady() =>
        NavIsReady ?? false;

    /// <summary>
    ///     获取导航网格的构建进度
    /// </summary>
    /// <returns></returns>
    public static float GetNavBuildProgress() =>
        NavBuildProgress ?? 0f;

    /// <summary>
    ///     重新加载导航网格
    /// </summary>
    public static void ReloadNav() =>
        NavReload?.InvokeFunc();

    /// <summary>
    ///     重新构建导航网格
    /// </summary>
    public static void RebuildNav() =>
        NavRebuild?.InvokeFunc();

    /// <summary>
    ///     寻路
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? Pathfind(Vector3 from, Vector3 to, bool fly = false) =>
        NavPathfind?.InvokeFunc(from, to, fly);

    /// <summary>
    ///     可取消的寻路
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? PathfindCancelable(Vector3 from, Vector3 to, bool fly, CancellationToken cancellationToken) =>
        NavPathfindCancelable?.InvokeFunc(from, to, fly, cancellationToken);

    /// <summary>
    ///     获取排队的寻路请求数量
    /// </summary>
    /// <returns></returns>
    public static int GetQueuedPathfindCount() =>
        NavPathfindNumQueued ?? 0;

    /// <summary>
    ///     构建导航网格的位图表示
    /// </summary>
    /// <param name="startingPos">起始位置</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <returns></returns>
    public static bool BuildNavBitmap(Vector3 startingPos, string filename, float pixelSize) =>
        NavBuildBitmap?.InvokeFunc(startingPos, filename, pixelSize) ?? false;

    /// <summary>
    ///     在限定范围内构建导航网格的位图表示
    /// </summary>
    /// <param name="startingPos">起始位置</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <param name="minBounds">最小边界</param>
    /// <param name="maxBounds">最大边界</param>
    /// <returns></returns>
    public static bool BuildNavBoundedBitmap(Vector3 startingPos, string filename, float pixelSize, Vector3 minBounds, Vector3 maxBounds) =>
        NavBuildBitmapBounded?.InvokeFunc(startingPos, filename, pixelSize, minBounds, maxBounds) ?? false;

    /// <summary>
    ///     检查是否启用了自动加载
    /// </summary>
    /// <returns></returns>
    public static bool GetIsAutoLoadNav() =>
        NavIsAutoLoad ?? false;

    /// <summary>
    ///     设置是否启用自动加载
    /// </summary>
    /// <param name="value">值</param>
    public static void SetIsAutoLoadNav(bool value) =>
        NavSetAutoLoad?.InvokeAction(value);

    /// <summary>
    ///     检查寻路是否正在进行中
    /// </summary>
    /// <returns></returns>
    public static bool GetIsNavPathfindInProgress() =>
        NavPathfindInProgress ?? false;

    /// <summary>
    ///     查询网格上最近的点
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="halfExtentXZ">XZ半区</param>
    /// <param name="halfExtentY">Y半区</param>
    /// <returns></returns>
    public static Vector3? QueryNearestPointOnMesh(Vector3 pos, float halfExtentXZ, float halfExtentY) =>
        QueryMeshNearestPoint?.InvokeFunc(pos, halfExtentXZ, halfExtentY);

    /// <summary>
    ///     查询地板上的点
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="allowUnlandable">允许无法降落</param>
    /// <param name="halfExtentXZ">XZ半区</param>
    /// <returns></returns>
    public static Vector3? QueryMeshPointOnFloor(Vector3 pos, bool allowUnlandable, float halfExtentXZ) =>
        QueryPointOnFloor?.InvokeFunc(pos, allowUnlandable, halfExtentXZ);

    /// <summary>
    ///     沿着既定路径点移动
    /// </summary>
    /// <param name="waypoints">路径点</param>
    /// <param name="fly">是否飞行</param>
    public static void PathfindWithPath(List<Vector3> waypoints, bool fly) =>
        PathMoveTo?.InvokeAction(waypoints, fly);

    /// <summary>
    ///     停止移动
    /// </summary>
    public static void StopPathfind() =>
        PathStop?.InvokeAction();

    /// <summary>
    ///     检查是否正在移动
    /// </summary>
    /// <returns></returns>
    public static bool GetIsPathfindRunning() =>
        PathIsRunning ?? false;

    /// <summary>
    ///     获取路径点的数量
    /// </summary>
    /// <returns></returns>
    public static int GetPathWayPointCount() =>
        PathNumWaypoints ?? 0;

    /// <summary>
    ///     获取路径点列表
    /// </summary>
    /// <returns></returns>
    public static List<Vector3> GetPathfindWayPoints() =>
        PathListWaypoints?.InvokeFunc() ?? [];

    /// <summary>
    ///     获取是否允许移动
    /// </summary>
    /// <returns></returns>
    public static bool GetIsPathfindMovementAllowed() =>
        PathGetMovementAllowed ?? false;

    /// <summary>
    ///     设置是否允许移动
    /// </summary>
    /// <param name="value">值</param>
    public static void SetIsPathfindMovementAllowed(bool value) =>
        PathSetMovementAllowed?.InvokeAction(value);

    /// <summary>
    ///     获取是否对齐镜头
    /// </summary>
    /// <returns></returns>
    public static bool GetIsPathfindAlignCamera() =>
        PathGetAlignCamera ?? false;

    /// <summary>
    ///     设置是否对齐镜头
    /// </summary>
    /// <param name="value">值</param>
    public static void SetIsPathfindAlignCamera(bool value) =>
        PathSetAlignCamera?.InvokeAction(value);

    /// <summary>
    ///     获取容差
    /// </summary>
    /// <returns></returns>
    public static float GetPathfindTolerance() =>
        PathGetTolerance ?? 0f;

    /// <summary>
    ///     设置容差
    /// </summary>
    /// <param name="tolerance">容差值</param>
    public static void SetPathfindTolerance(float tolerance) =>
        PathSetTolerance?.InvokeAction(tolerance);

    /// <summary>
    ///     寻路并移动到目标点
    /// </summary>
    /// <param name="pos">目标点</param>
    /// <param name="fly">是否飞行</param>
    /// <returns></returns>
    public static bool PathfindAndMoveTo(Vector3 pos, bool fly) =>
        SimpleMovePathfindAndMoveTo?.InvokeFunc(pos, fly) ?? false;

    /// <summary>
    ///     寻路并移动到目标点附近
    /// </summary>
    /// <param name="pos">目标点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="range">范围</param>
    /// <returns></returns>
    public static bool PathfindAndMoveToClosely(Vector3 pos, bool fly, float range) =>
        PathfindAndMoveToCloseTo?.InvokeFunc(pos, fly, range) ?? false;

    /// <summary>
    ///     检查寻路是否正在进行中
    /// </summary>
    /// <returns></returns>
    public static bool GetIsPathfindInProgress() =>
        PathfindInProgress ?? false;

    /// <summary>
    ///     取消所有查询
    /// </summary>
    public static void CancelAllQueries() =>
        PathfindCancelAll?.InvokeAction();

    /// <summary>
    ///     检查窗口是否打开
    /// </summary>
    /// <returns></returns>
    public static bool GetIsWindowOpen() =>
        WindowIsOpen ?? false;

    /// <summary>
    ///     设置窗口是否打开
    /// </summary>
    /// <param name="value">值</param>
    public static void SetIsWindowOpen(bool value) =>
        WindowSetOpen?.InvokeAction(value);

    /// <summary>
    ///     检查DTR栏信息是否显示
    /// </summary>
    /// <returns></returns>
    public static bool GetIsDTRShown() =>
        DTRIsShown ?? false;

    /// <summary>
    ///     设置DTR栏信息是否显示
    /// </summary>
    /// <param name="value">值</param>
    public static void SetIsDTRShown(bool value) =>
        DTRSetShown?.InvokeAction(value);
}

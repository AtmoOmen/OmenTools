using System.Numerics;
using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

// ReSharper disable once InconsistentNaming
public static class vnavmeshIPC
{
    public const string INTERNAL_NAME = "vnavmesh";

    public static bool IsPluginEnabled(bool onlyCN = true) =>
        IDalamudPluginInterface.Instance().IsPluginEnabled
        (
            INTERNAL_NAME,
            plugin => !onlyCN || plugin.Manifest.Author.Contains("AtmoOmen")
        );

    [IPCSubscriber("vnavmesh.Nav.IsReady", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavIsReadyIPC;

    [IPCSubscriber("vnavmesh.Nav.BuildProgress", DefaultValue = "-1")]
    private static IPCSubscriber<float>? NavBuildProgressIPC;

    [IPCSubscriber("vnavmesh.Nav.Reload")]
    private static IPCSubscriber<bool>? NavReloadIPC;

    [IPCSubscriber("vnavmesh.Nav.Rebuild")]
    private static IPCSubscriber<bool>? NavRebuildIPC;

    [IPCSubscriber("vnavmesh.Nav.Pathfind")]
    private static IPCSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>>? NavPathfindIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindWithTolerance")]
    private static IPCSubscriber<Vector3, Vector3, bool, float, Task<List<Vector3>>>? NavPathfindWithToleranceIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindAvoid")]
    private static IPCSubscriber<Vector3, Vector3, bool, Vector3, float, Task<List<Vector3>>>? NavPathfindAvoidIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindCancelable")]
    private static IPCSubscriber<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>>? NavPathfindCancelableIPC;

    [IPCSubscriber("vnavmesh.Nav.IsAutoLoad", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavIsAutoLoadIPC;

    [IPCSubscriber("vnavmesh.Nav.SetAutoLoad")]
    private static IPCSubscriber<bool, object>? NavSetAutoLoadIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindNumQueued", DefaultValue = "0")]
    private static IPCSubscriber<int>? NavPathfindNumQueuedIPC;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmap")]
    private static IPCSubscriber<Vector3, string, float, bool>? NavBuildBitmapIPC;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmapBounded")]
    private static IPCSubscriber<Vector3, string, float, Vector3, Vector3, bool>? NavBuildBitmapBoundedIPC;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmapMulti")]
    private static IPCSubscriber<List<Vector3>, string, float, bool>? NavBuildBitmapMultiIPC;

    [IPCSubscriber("vnavmesh.Nav.BuildBitmapMultiBounded")]
    private static IPCSubscriber<List<Vector3>, string, float, Vector3, Vector3, bool>? NavBuildBitmapMultiBoundedIPC;

    [IPCSubscriber("vnavmesh.Query.Mesh.NearestPoint")]
    private static IPCSubscriber<Vector3, float, float, Vector3?>? QueryMeshNearestPointIPC;

    [IPCSubscriber("vnavmesh.Query.Voxel.NearestPoint")]
    private static IPCSubscriber<Vector3, float, float, Vector3?>? QueryVoxelNearestPointIPC;

    [IPCSubscriber("vnavmesh.Query.Mesh.IsPointOnMesh")]
    private static IPCSubscriber<Vector3, float, bool, bool>? QueryMeshIsPointOnMeshIPC;

    [IPCSubscriber("vnavmesh.Query.Mesh.NearestPointReachable")]
    private static IPCSubscriber<Vector3, float, float, Vector3?>? QueryMeshNearestPointReachableIPC;

    [IPCSubscriber("vnavmesh.Query.Mesh.PointOnFloor")]
    private static IPCSubscriber<Vector3, bool, float, Vector3?>? QueryPointOnFloorIPC;

    [IPCSubscriber("vnavmesh.Query.Mesh.FlagToPoint")]
    private static IPCSubscriber<Vector3?>? QueryMeshFlagToPointIPC;

    [IPCSubscriber("vnavmesh.Path.MoveTo")]
    private static IPCSubscriber<List<Vector3>, bool, object>? PathMoveToIPC;

    [IPCSubscriber("vnavmesh.Path.Stop")]
    private static IPCSubscriber<object>? PathStopIPC;

    [IPCSubscriber("vnavmesh.Path.IsRunning", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathIsRunningIPC;

    [IPCSubscriber("vnavmesh.Path.NumWaypoints", DefaultValue = "0")]
    private static IPCSubscriber<int>? PathNumWaypointsIPC;

    [IPCSubscriber("vnavmesh.Path.ListWaypoints")]
    private static IPCSubscriber<List<Vector3>>? PathListWaypointsIPC;

    [IPCSubscriber("vnavmesh.Path.GetMovementAllowed", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathGetMovementAllowedIPC;

    [IPCSubscriber("vnavmesh.Path.SetMovementAllowed")]
    private static IPCSubscriber<bool, object>? PathSetMovementAllowedIPC;

    [IPCSubscriber("vnavmesh.Path.GetAlignCamera", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathGetAlignCameraIPC;

    [IPCSubscriber("vnavmesh.Path.SetAlignCamera")]
    private static IPCSubscriber<bool, object>? PathSetAlignCameraIPC;

    [IPCSubscriber("vnavmesh.Path.GetTolerance", DefaultValue = "0")]
    private static IPCSubscriber<float>? PathGetToleranceIPC;

    [IPCSubscriber("vnavmesh.Path.SetTolerance")]
    private static IPCSubscriber<float, object>? PathSetToleranceIPC;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindAndMoveTo")]
    private static IPCSubscriber<Vector3, bool, bool>? SimpleMovePathfindAndMoveToIPC;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindAndMoveCloseTo")]
    private static IPCSubscriber<Vector3, bool, float, bool>? PathfindAndMoveToCloseToIPC;

    [IPCSubscriber("vnavmesh.SimpleMove.PathfindInProgress", DefaultValue = "false")]
    private static IPCSubscriber<bool>? PathfindInProgressIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindInProgress", DefaultValue = "false")]
    private static IPCSubscriber<bool>? NavPathfindInProgressIPC;

    [IPCSubscriber("vnavmesh.Nav.PathfindCancelAll")]
    private static IPCSubscriber<object>? PathfindCancelAllIPC;

    [IPCSubscriber("vnavmesh.Window.IsOpen", DefaultValue = "false")]
    private static IPCSubscriber<bool>? WindowIsOpenIPC;

    [IPCSubscriber("vnavmesh.Window.SetOpen")]
    private static IPCSubscriber<bool, object>? WindowSetOpenIPC;

    [IPCSubscriber("vnavmesh.DTR.IsShown", DefaultValue = "false")]
    private static IPCSubscriber<bool>? DTRIsShownIPC;

    [IPCSubscriber("vnavmesh.DTR.SetShown")]
    private static IPCSubscriber<bool, object>? DTRSetShownIPC;

    /// <summary>
    ///     检查导航网格是否准备就绪
    /// </summary>
    public static bool IsNavReady =>
        NavIsReadyIPC?.TryInvokeFunc() ?? false;

    /// <summary>
    ///     获取导航网格的构建进度
    /// </summary>
    public static float NavBuildProgress =>
        NavBuildProgressIPC?.TryInvokeFunc() ?? 0f;

    /// <summary>
    ///     获取排队的寻路请求数量。
    /// </summary>
    public static int QueuedPathfindCount =>
        NavPathfindNumQueuedIPC?.TryInvokeFunc() ?? 0;

    /// <summary>
    ///     获取或设置是否自动加载导航网格。
    /// </summary>
    public static bool IsAutoLoadNav
    {
        get => NavIsAutoLoadIPC?.TryInvokeFunc() ?? false;
        set => NavSetAutoLoadIPC?.TryInvokeAction(value);
    }

    /// <summary>
    ///     检查导航网格寻路是否正在进行中。
    /// </summary>
    public static bool IsNavPathfindInProgress =>
        NavPathfindInProgressIPC?.TryInvokeFunc() ?? false;

    /// <summary>
    ///     检查是否正在移动。
    /// </summary>
    public static bool IsPathfindRunning =>
        PathIsRunningIPC?.TryInvokeFunc() ?? false;

    /// <summary>
    ///     获取路径点的数量。
    /// </summary>
    public static int PathWayPointCount =>
        PathNumWaypointsIPC?.TryInvokeFunc() ?? 0;

    /// <summary>
    ///     获取路径点列表。
    /// </summary>
    public static List<Vector3> PathfindWayPoints =>
        PathListWaypointsIPC?.TryInvokeFunc() ?? [];

    /// <summary>
    ///     获取或设置是否允许寻路移动。
    /// </summary>
    public static bool IsPathfindMovementAllowed
    {
        get => PathGetMovementAllowedIPC?.TryInvokeFunc() ?? false;
        set => PathSetMovementAllowedIPC?.TryInvokeAction(value);
    }

    /// <summary>
    ///     获取或设置寻路时是否对齐镜头。
    /// </summary>
    public static bool IsPathfindAlignCamera
    {
        get => PathGetAlignCameraIPC?.TryInvokeFunc() ?? false;
        set => PathSetAlignCameraIPC?.TryInvokeAction(value);
    }

    /// <summary>
    ///     获取或设置寻路容差。
    /// </summary>
    /// <remarks>设置仅单次有效。</remarks>
    public static float PathfindTolerance
    {
        get => PathGetToleranceIPC?.TryInvokeFunc() ?? 0f;
        set => PathSetToleranceIPC?.TryInvokeAction(value);
    }

    /// <summary>
    ///     检查寻路是否正在进行中。
    /// </summary>
    public static bool IsPathfindInProgress =>
        PathfindInProgressIPC?.TryInvokeFunc() ?? false;

    /// <summary>
    ///     是否打开了主界面。
    /// </summary>
    public static bool IsWindowOpen
    {
        get => WindowIsOpenIPC?.TryInvokeFunc() ?? false;
        set => WindowSetOpenIPC?.TryInvokeFunc(value);
    }

    /// <summary>
    ///     “基本信息”界面条目是否显示。
    /// </summary>
    public static bool IsDTRShown
    {
        get => DTRIsShownIPC?.TryInvokeFunc() ?? false;
        set => DTRSetShownIPC?.TryInvokeAction(value);
    }

    /// <summary>
    ///     重新加载导航。
    /// </summary>
    public static void ReloadNav() =>
        NavReloadIPC?.TryInvokeFunc();

    /// <summary>
    ///     重新构建导航。
    /// </summary>
    public static void RebuildNav() =>
        NavRebuildIPC?.TryInvokeFunc();

    /// <summary>
    ///     算路。
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? Pathfind
    (
        Vector3 from,
        Vector3 to,
        bool    fly = false
    ) =>
        NavPathfindIPC?.TryInvokeFunc(from, to, fly);

    /// <summary>
    ///     带容差的算路。
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="range">容差范围</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? PathfindWithTolerance
    (
        Vector3 from,
        Vector3 to,
        bool    fly,
        float   range
    ) =>
        NavPathfindWithToleranceIPC?.TryInvokeFunc(from, to, fly, range);

    /// <summary>
    ///     避开指定区域的算路。
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="avoidCenter">规避中心</param>
    /// <param name="avoidRadius">规避半径</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? PathfindAvoid
    (
        Vector3 from,
        Vector3 to,
        bool    fly,
        Vector3 avoidCenter,
        float   avoidRadius
    ) =>
        NavPathfindAvoidIPC?.TryInvokeFunc(from, to, fly, avoidCenter, avoidRadius);

    /// <summary>
    ///     可取消的算路。
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static Task<List<Vector3>>? PathfindCancelable
    (
        Vector3           from,
        Vector3           to,
        bool              fly,
        CancellationToken cancellationToken
    ) =>
        NavPathfindCancelableIPC?.TryInvokeFunc(from, to, fly, cancellationToken);

    /// <summary>
    ///     构建表示导航的位图。
    /// </summary>
    /// <param name="startingPos">起始位置</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <returns></returns>
    public static bool BuildNavBitmap
    (
        Vector3 startingPos,
        string  filename,
        float   pixelSize
    ) =>
        NavBuildBitmapIPC?.TryInvokeFunc(startingPos, filename, pixelSize) ?? false;

    /// <summary>
    ///     在限定范围内构建表示导航的位图。
    /// </summary>
    /// <param name="startingPos">起始位置</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <param name="minBounds">最小边界</param>
    /// <param name="maxBounds">最大边界</param>
    /// <returns></returns>
    public static bool BuildNavBoundedBitmap
    (
        Vector3 startingPos,
        string  filename,
        float   pixelSize,
        Vector3 minBounds,
        Vector3 maxBounds
    ) =>
        NavBuildBitmapBoundedIPC?.TryInvokeFunc(startingPos, filename, pixelSize, minBounds, maxBounds) ?? false;

    /// <summary>
    ///     从多个起始位置构建表示导航的位图。
    /// </summary>
    /// <param name="startingPositions">起始位置列表</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <returns></returns>
    public static bool BuildNavMultiBitmap
    (
        List<Vector3> startingPositions,
        string        filename,
        float         pixelSize
    ) =>
        NavBuildBitmapMultiIPC?.TryInvokeFunc(startingPositions, filename, pixelSize) ?? false;

    /// <summary>
    ///     在限定范围内从多个起始位置构建表示导航的位图。
    /// </summary>
    /// <param name="startingPositions">起始位置列表</param>
    /// <param name="filename">文件名</param>
    /// <param name="pixelSize">像素大小</param>
    /// <param name="minBounds">最小边界</param>
    /// <param name="maxBounds">最大边界</param>
    /// <returns></returns>
    public static bool BuildNavMultiBoundedBitmap
    (
        List<Vector3> startingPositions,
        string        filename,
        float         pixelSize,
        Vector3       minBounds,
        Vector3       maxBounds
    ) =>
        NavBuildBitmapMultiBoundedIPC?.TryInvokeFunc(startingPositions, filename, pixelSize, minBounds, maxBounds) ?? false;

    /// <summary>
    ///     查询指定位置在网格（地面）上最近的坐标。
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="halfExtentXZ">XZ半区</param>
    /// <param name="halfExtentY">Y半区</param>
    /// <returns></returns>
    public static Vector3? QueryNearestPointOnMesh
    (
        Vector3 pos,
        float   halfExtentXZ = 2048,
        float   halfExtentY  = 2048
    ) =>
        QueryMeshNearestPointIPC?.TryInvokeFunc(pos, halfExtentXZ, halfExtentY);

    /// <summary>
    ///     查询指定位置在体素（空中）内最近的坐标。
    /// </summary>
    /// <param name="point">目标位置，同时作为搜索范围的中心。</param>
    /// <param name="halfExtentXZ">搜索范围在 X、Z 方向的半长度。</param>
    /// <param name="halfExtentY">搜索范围在 Y 方向的半长度。</param>
    /// <returns>最近的可达体素点；未找到、导航数据未就绪或接口不可用时返回 <see langword="null"/>。</returns>
    public static Vector3? QueryNearestPointInVoxel
    (
        Vector3 point,
        float   halfExtentXZ = 2048,
        float   halfExtentY  = 2048
    ) =>
        QueryVoxelNearestPointIPC?.TryInvokeFunc(point, halfExtentXZ, halfExtentY);

    /// <summary>
    ///     检查指定位置是否在网格（地面）上。
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="halfExtentY">Y半区</param>
    /// <param name="allowUnreachable">允许网格不可达</param>
    /// <returns></returns>
    public static bool QueryIsPointOnMesh
    (
        Vector3 pos,
        float   halfExtentY      = 2048,
        bool    allowUnreachable = true
    ) =>
        QueryMeshIsPointOnMeshIPC?.TryInvokeFunc(pos, halfExtentY, allowUnreachable) ?? false;

    /// <summary>
    ///     查询网格（地面）上距离指定位置最近的可通达坐标。
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="halfExtentXZ">XZ半区</param>
    /// <param name="halfExtentY">Y半区</param>
    /// <returns></returns>
    public static Vector3? QueryNearestPointReachable
    (
        Vector3 pos,
        float   halfExtentXZ = 2048,
        float   halfExtentY  = 2048
    ) =>
        QueryMeshNearestPointReachableIPC?.TryInvokeFunc(pos, halfExtentXZ, halfExtentY);

    /// <summary>
    ///     将地图标点转为世界坐标。
    /// </summary>
    /// <returns></returns>
    public static Vector3? QueryFlagToPoint() =>
        QueryMeshFlagToPointIPC?.TryInvokeFunc();

    /// <summary>
    ///     查询指定位置在地面上的坐标。不一定在网格上。
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="allowUnlandable">允许无法降落</param>
    /// <param name="halfExtentXZ">XZ半区</param>
    /// <returns></returns>
    public static Vector3? QueryPointOnFloor
    (
        Vector3 pos,
        bool    allowUnlandable,
        float   halfExtentXZ
    ) =>
        QueryPointOnFloorIPC?.TryInvokeFunc(pos, allowUnlandable, halfExtentXZ);

    /// <summary>
    ///     沿着既定路径寻路。
    /// </summary>
    /// <param name="waypoints">路径位置</param>
    /// <param name="fly">是否飞行</param>
    public static void PathfindWithPath
    (
        List<Vector3> waypoints,
        bool          fly
    ) =>
        PathMoveToIPC?.TryInvokeAction(waypoints, fly);

    /// <summary>
    ///     停止移动。
    /// </summary>
    public static void StopPathfind() =>
        PathStopIPC?.TryInvokeAction();

    /// <summary>
    ///     算路并寻路到目标位置。
    /// </summary>
    /// <param name="pos">目标位置</param>
    /// <param name="fly">是否飞行</param>
    /// <returns></returns>
    public static bool PathfindAndMoveTo
    (
        Vector3 pos,
        bool    fly
    ) =>
        SimpleMovePathfindAndMoveToIPC?.TryInvokeFunc(pos, fly) ?? false;

    /// <summary>
    ///     算路并寻路到目标位置附近。
    /// </summary>
    /// <param name="pos">目标点</param>
    /// <param name="fly">是否飞行</param>
    /// <param name="range">范围</param>
    /// <returns></returns>
    public static bool PathfindAndMoveToClosely
    (
        Vector3 pos,
        bool    fly,
        float   range
    ) =>
        PathfindAndMoveToCloseToIPC?.TryInvokeFunc(pos, fly, range) ?? false;

    /// <summary>
    ///     取消所有算路任务。
    /// </summary>
    public static void CancelAllQueries() =>
        PathfindCancelAllIPC?.TryInvokeAction();
}

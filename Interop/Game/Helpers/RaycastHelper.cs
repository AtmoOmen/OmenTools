using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     封装游戏 BGCollision 场景射线查询, 提供地面, 天花板, 视线, 推离障碍, 材质过滤和体积扫掠等常用检测入口
/// </summary>
/// <remarks>
///     所有坐标均使用游戏世界坐标, 即 <see cref="Vector3.X" /> 和 <see cref="Vector3.Z" /> 为水平面, <see cref="Vector3.Y" /> 为高度<br />
///     底层依赖 <see cref="Framework.Instance" /> 的 <c>BGCollisionModule</c>, 未进入游戏场景, 场景未加载完成或碰撞模块不可用时返回未命中<br />
///     默认层掩码为 1, 其他层或材质需求通过 <see cref="RaycastOptions" /> 指定<br />
///     命中查询会遍历当前 SceneManager 中全部场景, 并返回最近命中点
/// </remarks>
public static unsafe class RaycastHelper
{
    /// <summary>
    ///     获取指定位置下方地面高度
    /// </summary>
    /// <param name="position">待检测的世界坐标, 方法会从该位置上方 0.5 米处向下发射射线</param>
    /// <param name="groundY">命中时为地面高度, 未命中时回填 <paramref name="position" /> 的 Y 值</param>
    /// <param name="maxDistance">向下检测的最大距离, 默认 1000</param>
    /// <returns>命中地面或任意向下碰撞体时返回 true, 碰撞模块不可用或未命中时返回 false</returns>
    /// <remarks>
    ///     适合只需要高度数值的逻辑, 如贴地, 落点修正, 简单地面判断<br />
    ///     需要命中法线, 材质或距离时改用 <see cref="TryGetGroundHit" />
    /// </remarks>
    public static bool TryGetGroundHeight(Vector3 position, out float groundY, float maxDistance = DEFAULT_MAX_DISTANCE)
    {
        if (TryGetGroundHit(position, out var hit, maxDistance))
        {
            groundY = hit.Point.Y;
            return true;
        }

        groundY = position.Y;
        return false;
    }

    /// <summary>
    ///     获取指定位置下方最近的地面命中信息
    /// </summary>
    /// <param name="position">待检测的世界坐标</param>
    /// <param name="hitInfo">命中详情, 包含坐标, 法线, 距离, 材质和碰撞体指针</param>
    /// <param name="maxDistance">向下检测的最大距离, 默认 1000</param>
    /// <param name="originLift">射线起点相对 <paramref name="position" /> 上抬的高度, 用于避免起点贴在地面内部</param>
    /// <param name="options">射线选项, 可指定层掩码, SweepSphere, 忽略水平面和材质过滤</param>
    /// <returns>命中时返回 true, 未命中或输入无效时返回 false</returns>
    /// <remarks>
    ///     适合需要完整 <see cref="RaycastHit" /> 的逻辑, 如坡度判断, 材质判断, 命中距离排序<br />
    ///     <paramref name="originLift" /> 过小可能从碰撞面内部发射, 过大可能命中头顶结构下表面
    /// </remarks>
    public static bool TryGetGroundHit
    (
        Vector3        position,
        out RaycastHit hitInfo,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        float          originLift  = DEFAULT_GROUND_ORIGIN_LIFT,
        RaycastOptions options     = default
    )
    {
        var origin = position with { Y = position.Y + originLift };
        return TryRaycast(origin, -Vector3.UnitY, out hitInfo, maxDistance, options);
    }

    /// <summary>
    ///     获取指定位置下方地面坐标
    /// </summary>
    /// <param name="position">待检测的世界坐标</param>
    /// <param name="groundPosition">命中时为地面坐标, 未命中时回填原始 <paramref name="position" /></param>
    /// <param name="maxDistance">向下检测的最大距离, 默认 1000</param>
    /// <param name="originLift">射线起点相对 <paramref name="position" /> 上抬的高度</param>
    /// <param name="options">射线选项</param>
    /// <returns>命中时返回 true, 未命中时返回 false</returns>
    /// <remarks>
    ///     适合需要把点投影到地面的场景, 如目的地落地, 地图采样, 路径点修正<br />
    ///     只需要 Y 值时使用 <see cref="TryGetGroundHeight" /> 更直接
    /// </remarks>
    public static bool TryGetGroundPosition
    (
        Vector3        position,
        out Vector3    groundPosition,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        float          originLift  = DEFAULT_GROUND_ORIGIN_LIFT,
        RaycastOptions options     = default
    )
    {
        if (TryGetGroundHit(position, out var hitInfo, maxDistance, originLift, options))
        {
            groundPosition = hitInfo.Point;
            return true;
        }

        groundPosition = position;
        return false;
    }

    /// <summary>
    ///     判断两点之间是否无遮挡
    /// </summary>
    /// <param name="from">射线起点</param>
    /// <param name="to">射线终点</param>
    /// <returns>两点间未命中碰撞体时返回 true, 命中碰撞体或坐标无效时返回 false</returns>
    /// <remarks>
    ///     适合普通视线, 可交互路径, 简单直连可达性判断<br />
    ///     两点距离小于 0.001 时视为同一点并返回 true<br />
    ///     使用默认层掩码 1, 需要材质或层过滤时使用带 <see cref="RaycastOptions" /> 的重载
    /// </remarks>
    public static bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        if (!IsFinite(from) || !IsFinite(to))
            return false;

        if (!TryGetRay(from, to, out var direction, out var distance))
            return true;

        return !Intersects(from, direction, distance);
    }

    /// <summary>
    ///     按指定选项判断两点之间是否无遮挡
    /// </summary>
    /// <param name="from">射线起点</param>
    /// <param name="to">射线终点</param>
    /// <param name="options">射线选项, 可限制层, 材质或启用特殊算法</param>
    /// <returns>按指定选项未命中碰撞体时返回 true, 命中碰撞体或坐标无效时返回 false</returns>
    /// <remarks>
    ///     适合不同系统对视线定义不一致的场景, 如忽略特定材质, 只检测某些层, 或用 SweepSphere 近似角色半径<br />
    ///     <paramref name="options" /> 启用材质过滤时, 只有匹配材质的碰撞体会阻断视线
    /// </remarks>
    public static bool HasLineOfSight(Vector3 from, Vector3 to, RaycastOptions options)
    {
        if (!IsFinite(from) || !IsFinite(to))
            return false;

        if (!TryGetRay(from, to, out var direction, out var distance))
            return true;

        return !Intersects(from, direction, distance, options);
    }

    /// <summary>
    ///     获取指定位置上方天花板高度
    /// </summary>
    /// <param name="position">待检测的世界坐标</param>
    /// <param name="ceilingY">命中时为天花板高度, 未命中时为 <paramref name="position" /> 的 Y 值加 <paramref name="maxDistance" /></param>
    /// <param name="maxDistance">向上检测的最大距离, 默认 1000</param>
    /// <returns>命中上方碰撞体时返回 true, 未命中或碰撞模块不可用时返回 false</returns>
    /// <remarks>
    ///     适合限制角色升高, 检测洞窟顶部, 判断可飞行或可跳跃空间<br />
    ///     需要法线, 材质或命中距离时使用 <see cref="TryGetCeilingHit" />
    /// </remarks>
    public static bool TryGetCeilingHeight
    (
        Vector3   position,
        out float ceilingY,
        float     maxDistance = DEFAULT_CEILING_DISTANCE
    )
    {
        if (TryGetCeilingHit(position, out var hit, maxDistance))
        {
            ceilingY = hit.Point.Y;
            return true;
        }

        ceilingY = position.Y + maxDistance;
        return false;
    }

    /// <summary>
    ///     获取指定位置上方最近的天花板命中信息
    /// </summary>
    /// <param name="position">射线起点</param>
    /// <param name="hitInfo">命中详情</param>
    /// <param name="maxDistance">向上检测的最大距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>命中上方碰撞体时返回 true, 未命中时返回 false</returns>
    /// <remarks>
    ///     适合需要判断顶部材质, 顶部距离或顶部法线的场景<br />
    ///     检测方向固定为 <see cref="Vector3.UnitY" />, 自定义方向请使用 <see cref="TryRaycast" />
    /// </remarks>
    public static bool TryGetCeilingHit
    (
        Vector3        position,
        out RaycastHit hitInfo,
        float          maxDistance = DEFAULT_CEILING_DISTANCE,
        RaycastOptions options     = default
    ) =>
        TryRaycast(position, Vector3.UnitY, out hitInfo, maxDistance, options);

    /// <summary>
    ///     获取指定位置上方天花板坐标
    /// </summary>
    /// <param name="position">待检测的世界坐标</param>
    /// <param name="ceilingPosition">命中时为天花板坐标, 未命中时回填 Y 值增加 <paramref name="maxDistance" /> 的坐标</param>
    /// <param name="maxDistance">向上检测的最大距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>命中时返回 true, 未命中时返回 false</returns>
    /// <remarks>
    ///     适合需要把点投影到上方碰撞面的场景, 如顶部约束, 空间高度采样, 上方落点修正<br />
    ///     只需要 Y 值时使用 <see cref="TryGetCeilingHeight" /> 更直接
    /// </remarks>
    public static bool TryGetCeilingPosition
    (
        Vector3        position,
        out Vector3    ceilingPosition,
        float          maxDistance = DEFAULT_CEILING_DISTANCE,
        RaycastOptions options     = default
    )
    {
        if (TryGetCeilingHit(position, out var hitInfo, maxDistance, options))
        {
            ceilingPosition = hitInfo.Point;
            return true;
        }

        ceilingPosition = position with { Y = position.Y + maxDistance };
        return false;
    }

    /// <summary>
    ///     获取指定位置垂直上下方向最近的碰撞命中
    /// </summary>
    /// <param name="position">上下检测的中心点</param>
    /// <param name="hitInfo">最近命中的碰撞详情</param>
    /// <param name="downDistance">向下检测的最大距离, 默认 1000</param>
    /// <param name="upDistance">向上检测的最大距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>上下任意方向命中时返回 true, 两个方向都未命中时返回 false</returns>
    /// <remarks>
    ///     适合需要把点吸附到最近垂直表面的场景, 如上下层判断, 悬空点修正, 近距离空间约束<br />
    ///     下方和上方同时命中时比较 <see cref="RaycastHit.Distance" /> 并返回最近者
    /// </remarks>
    public static bool TryGetNearestVerticalHit
    (
        Vector3        position,
        out RaycastHit hitInfo,
        float          downDistance = DEFAULT_MAX_DISTANCE,
        float          upDistance   = DEFAULT_CEILING_DISTANCE,
        RaycastOptions options      = default
    )
    {
        var hasDownHit = TryRaycast(position + new Vector3(0, DEFAULT_GROUND_ORIGIN_LIFT, 0), -Vector3.UnitY, out var downHit, downDistance, options);
        var hasUpHit   = TryRaycast(position,                                                 Vector3.UnitY,  out var upHit,   upDistance,   options);

        if (hasDownHit && hasUpHit)
        {
            hitInfo = downHit.Distance <= upHit.Distance ? downHit : upHit;
            return true;
        }

        if (hasDownHit)
        {
            hitInfo = downHit;
            return true;
        }

        hitInfo = upHit;
        return hasUpHit;
    }

    /// <summary>
    ///     获取从起点前往目标点时的安全位置
    /// </summary>
    /// <param name="from">移动或检测起点</param>
    /// <param name="to">期望到达的位置</param>
    /// <returns>路径无阻挡时返回 <paramref name="to" />, 路径被阻挡时返回障碍前方的推离位置</returns>
    /// <remarks>
    ///     适合沿直线移动前的简化安全点计算<br />
    ///     该重载不暴露是否命中, 需要区分命中和未命中时使用 <see cref="TryGetSafePosition" />
    /// </remarks>
    public static Vector3 GetSafePosition(Vector3 from, Vector3 to) =>
        TryGetSafePosition(from, to, out var safePosition) ? safePosition : to;

    /// <summary>
    ///     尝试获取从起点前往目标点时的安全位置
    /// </summary>
    /// <param name="from">移动或检测起点</param>
    /// <param name="to">期望到达的位置</param>
    /// <param name="safePosition">命中时为障碍前方并沿水平法线推开的坐标, 未命中时为 <paramref name="to" /></param>
    /// <param name="clearance">与障碍保持的距离, 负值会按 0 处理</param>
    /// <param name="options">射线选项</param>
    /// <returns>路径被碰撞体阻挡时返回 true, 路径通畅或输入无效时返回 false</returns>
    /// <remarks>
    ///     适合寻路, 自动移动, 目标点修正等需要停在障碍前的场景<br />
    ///     推离方向来自命中法线的水平分量, 垂直法线不足时只按距离回退, 不额外水平推开
    /// </remarks>
    public static bool TryGetSafePosition
    (
        Vector3        from,
        Vector3        to,
        out Vector3    safePosition,
        float          clearance = DEFAULT_SAFE_CLEARANCE,
        RaycastOptions options   = default
    )
    {
        safePosition = to;

        if (!TryGetRay(from, to, out var direction, out var distance))
            return false;

        if (!TryRaycast(from, direction, out var hitInfo, distance, options))
            return false;

        var safeDistance = MathF.Max(0, hitInfo.Distance - MathF.Max(0, clearance));
        safePosition = from + direction * safeDistance;

        var pushDirection = hitInfo.Normal with { Y = 0 };
        if (TryNormalize(pushDirection, out var normalizedPushDirection))
            safePosition += normalizedPushDirection * clearance;

        return true;
    }

    /// <summary>
    ///     在两点之间发射射线并返回最近命中
    /// </summary>
    /// <param name="from">射线起点</param>
    /// <param name="to">射线终点</param>
    /// <param name="hitInfo">命中详情</param>
    /// <param name="options">射线选项</param>
    /// <returns>两点之间命中碰撞体时返回 true, 未命中, 坐标无效或两点过近时返回 false</returns>
    /// <remarks>
    ///     适合调用方只知道起点和终点的场景, 如目标可达检测, 交互线段检测, 技能落点阻挡检测<br />
    ///     最大距离自动使用两点间距离, 自定义方向和距离请使用 <see cref="TryRaycast" />
    /// </remarks>
    public static bool TryRaycastBetween
    (
        Vector3        from,
        Vector3        to,
        out RaycastHit hitInfo,
        RaycastOptions options = default
    )
    {
        hitInfo = default;

        return TryGetRay(from, to, out var direction, out var distance) &&
               TryRaycast(from, direction, out hitInfo, distance, options);
    }

    /// <summary>
    ///     从指定起点按方向发射标准射线并返回最近命中
    /// </summary>
    /// <param name="origin">射线起点</param>
    /// <param name="direction">射线方向, 方法内部会归一化</param>
    /// <param name="hitInfo">命中详情</param>
    /// <param name="maxDistance">最大检测距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>命中时返回 true, 未命中, 场景不可用或输入无效时返回 false</returns>
    /// <remarks>
    ///     适合通用碰撞检测, 可完全控制方向, 距离, 层和材质<br />
    ///     <paramref name="direction" /> 长度小于 0.001, 坐标含 NaN 或无穷值时直接返回 false<br />
    ///     需要角色半径近似或更宽容的路径检测时使用 <see cref="TrySweepSphere" />
    /// </remarks>
    public static bool TryRaycast
    (
        Vector3        origin,
        Vector3        direction,
        out RaycastHit hitInfo,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        RaycastOptions options     = default
    ) =>
        RaycastScene(origin, direction, maxDistance, out hitInfo, options, true, options.GetMode());

    /// <summary>
    ///     从指定起点按方向执行球体扫掠并返回最近命中
    /// </summary>
    /// <param name="origin">扫掠起点</param>
    /// <param name="direction">扫掠方向, 方法内部会归一化</param>
    /// <param name="hitInfo">命中详情</param>
    /// <param name="maxDistance">最大扫掠距离, 默认 1000</param>
    /// <param name="options">射线选项, 方法会强制追加 <see cref="RaycastMode.SweepSphere" /></param>
    /// <returns>命中时返回 true, 未命中或输入无效时返回 false</returns>
    /// <remarks>
    ///     适合角色体积相关检测, 如移动通道, 潜水区, 贴边容错和窄缝阻挡判断<br />
    ///     SweepSphere 的半径由游戏原生算法决定, 调用方只传方向和距离
    /// </remarks>
    public static bool TrySweepSphere
    (
        Vector3        origin,
        Vector3        direction,
        out RaycastHit hitInfo,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        RaycastOptions options     = default
    ) =>
        RaycastScene(origin, direction, maxDistance, out hitInfo, options, true, options.GetMode() | RaycastMode.SweepSphere);

    /// <summary>
    ///     判断指定射线范围内是否存在任意碰撞体
    /// </summary>
    /// <param name="origin">射线起点</param>
    /// <param name="direction">射线方向, 方法内部会归一化</param>
    /// <param name="maxDistance">最大检测距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>存在任意命中时返回 true, 未命中或输入无效时返回 false</returns>
    /// <remarks>
    ///     适合只关心是否被阻挡的高频检测, 如视线阻挡, 可达性筛选, 快速布尔判断<br />
    ///     该方法命中后立即返回, 不保证取得最近碰撞, 需要命中详情时使用 <see cref="TryRaycast" />
    /// </remarks>
    public static bool Intersects
    (
        Vector3        origin,
        Vector3        direction,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        RaycastOptions options     = default
    ) =>
        RaycastScene(origin, direction, maxDistance, out _, options, false, options.GetMode());

    /// <summary>
    ///     判断指定点是否处于任意碰撞体内部
    /// </summary>
    /// <param name="position">待检测的世界坐标</param>
    /// <param name="options">检测选项, 当前主要使用 <see cref="RaycastOptions.LayerMask" /></param>
    /// <returns>点位落在匹配层的碰撞体内部时返回 true, 未包含或场景不可用时返回 false</returns>
    /// <remarks>
    ///     适合出生点, 目标点, 瞬移点或采样点的穿模检测<br />
    ///     该方法不是射线检测, 材质过滤不会参与原生 FindContainingCollidersCheckLayer 判断
    /// </remarks>
    public static bool IsInsideCollision(Vector3 position, RaycastOptions options = default)
    {
        if (!TryGetSceneManager(out var sceneManager) || !IsFinite(position))
            return false;

        foreach (var scene in sceneManager->Scenes)
        {
            var result = new SceneWrapper.ColliderList();
            if (scene->FindContainingCollidersCheckLayer(&result, options.GetLayerMask(), &position))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     获取指定射线方向上的最近碰撞距离
    /// </summary>
    /// <param name="origin">射线起点</param>
    /// <param name="direction">射线方向, 方法内部会归一化</param>
    /// <param name="distance">命中时为命中距离, 未命中时为 <paramref name="maxDistance" /></param>
    /// <param name="maxDistance">最大检测距离, 默认 1000</param>
    /// <param name="options">射线选项</param>
    /// <returns>命中时返回 true, 未命中时返回 false</returns>
    /// <remarks>
    ///     适合只需要距离的逻辑, 如前方余量, 接近墙体, 自动停步或空间宽度估算<br />
    ///     需要坐标, 法线或材质时使用 <see cref="TryRaycast" />
    /// </remarks>
    public static bool TryGetCollisionDistance
    (
        Vector3        origin,
        Vector3        direction,
        out float      distance,
        float          maxDistance = DEFAULT_MAX_DISTANCE,
        RaycastOptions options     = default
    )
    {
        if (TryRaycast(origin, direction, out var hitInfo, maxDistance, options))
        {
            distance = hitInfo.Distance;
            return true;
        }

        distance = maxDistance;
        return false;
    }

    /// <summary>
    ///     执行底层 SceneWrapper.Raycast 调用
    /// </summary>
    /// <param name="origin">射线或扫掠起点</param>
    /// <param name="direction">射线或扫掠方向, 方法内部会归一化</param>
    /// <param name="maxDistance">最大检测距离</param>
    /// <param name="hitInfo">命中详情</param>
    /// <param name="options">射线选项</param>
    /// <param name="findClosest">true 时遍历所有场景取最近命中, false 时命中任意场景立即返回</param>
    /// <param name="mode">原生 RaycastParams.Algorithm 位标志</param>
    /// <returns>命中时返回 true, 输入无效或场景不可用时返回 false</returns>
    /// <remarks>
    ///     该方法负责把托管选项转换为 FFXIVClientStructs 的指针参数<br />
    ///     <paramref name="findClosest" /> 用于区分详情查询和布尔查询, 高频布尔判断可减少不必要遍历
    /// </remarks>
    private static bool RaycastScene
    (
        Vector3        origin,
        Vector3        direction,
        float          maxDistance,
        out RaycastHit hitInfo,
        RaycastOptions options,
        bool           findClosest,
        RaycastMode    mode
    )
    {
        hitInfo = default;

        if (!TryGetSceneManager(out var sceneManager)             ||
            !TryNormalize(direction, out var normalizedDirection) ||
            !IsFinite(origin)                                     ||
            !float.IsFinite(maxDistance)                          ||
            maxDistance <= MIN_RAY_DISTANCE) return false;

        var origin4 = new Vector4(origin, 1);
        var maxDist = maxDistance;
        var materialFilter = new RaycastMaterialFilter
        {
            Mask  = options.MaterialMask,
            Value = options.MaterialValue
        };

        var args = new RaycastParams
        {
            Algorithm           = (int)mode,
            Origin              = &origin4,
            Direction           = &normalizedDirection,
            MaxDistance         = &maxDist,
            MaxPlaneNormalY     = options.MaxPlaneNormalY,
            MaterialFilter      = (mode & RaycastMode.MaterialFilter) == 0 ? null : &materialFilter,
            ObjectMaterialMask  = options.ObjectMaterialMask,
            ObjectMaterialValue = options.ObjectMaterialValue
        };

        var layerMask    = options.GetLayerMask();
        var bestDistance = float.PositiveInfinity;
        var hasHit       = false;

        foreach (var scene in sceneManager->Scenes)
        {
            var hit = new RaycastHit();
            if (!scene->Raycast(&hit, layerMask, &args))
                continue;

            if (!findClosest)
            {
                hitInfo = hit;
                return true;
            }

            if (hit.Distance >= bestDistance)
                continue;

            bestDistance = hit.Distance;
            hitInfo      = hit;
            hasHit       = true;
        }

        return hasHit;
    }

    /// <summary>
    ///     获取当前 BGCollision 的场景管理器
    /// </summary>
    /// <param name="sceneManager">可用时为当前 SceneManager 指针</param>
    /// <returns>碰撞模块和场景列表均可用时返回 true</returns>
    /// <remarks>
    ///     指针只在当前调用期间使用, 不做缓存<br />
    ///     场景切换, 载入界面或模块未初始化时可能返回 false
    /// </remarks>
    private static bool TryGetSceneManager(out SceneManager* sceneManager)
    {
        sceneManager = null;

        var framework = Framework.Instance();
        if (framework == null)
            return false;

        var module = framework->BGCollisionModule;
        if (module == null || module->SceneManager == null || module->SceneManager->NumScenes == 0)
            return false;

        sceneManager = module->SceneManager;
        return true;
    }

    /// <summary>
    ///     把两点转换为射线方向和距离
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="direction">输出归一化方向</param>
    /// <param name="distance">输出两点距离</param>
    /// <returns>坐标有效且两点距离大于最小射线距离时返回 true</returns>
    /// <remarks>
    ///     适合所有两点式 API 复用, 避免重复开方和零长度方向归一化
    /// </remarks>
    private static bool TryGetRay(Vector3 from, Vector3 to, out Vector3 direction, out float distance)
    {
        direction = default;
        distance  = 0;

        if (!IsFinite(from) || !IsFinite(to))
            return false;

        var delta         = to - from;
        var lengthSquared = delta.LengthSquared();
        if (lengthSquared <= MIN_RAY_DISTANCE_SQUARED || !float.IsFinite(lengthSquared))
            return false;

        distance  = MathF.Sqrt(lengthSquared);
        direction = delta / distance;
        return true;
    }

    /// <summary>
    ///     安全归一化向量
    /// </summary>
    /// <param name="vector">待归一化向量</param>
    /// <param name="normalized">输出归一化向量</param>
    /// <returns>向量长度足够且全部分量有效时返回 true</returns>
    /// <remarks>
    ///     用于避免 <see cref="Vector3.Normalize(Vector3)" /> 在零向量或非法值上产生 NaN
    /// </remarks>
    private static bool TryNormalize(Vector3 vector, out Vector3 normalized)
    {
        normalized = default;

        var lengthSquared = vector.LengthSquared();
        if (lengthSquared <= MIN_RAY_DISTANCE_SQUARED || !float.IsFinite(lengthSquared))
            return false;

        normalized = vector / MathF.Sqrt(lengthSquared);
        return true;
    }

    /// <summary>
    ///     判断坐标分量是否均为有限值
    /// </summary>
    /// <param name="vector">待检查坐标</param>
    /// <returns>X, Y, Z 均不是 NaN 或无穷值时返回 true</returns>
    /// <remarks>
    ///     适合所有进入原生碰撞查询前的输入检查, 防止非法浮点值传入游戏函数
    /// </remarks>
    private static bool IsFinite(Vector3 vector) =>
        float.IsFinite(vector.X) &&
        float.IsFinite(vector.Y) &&
        float.IsFinite(vector.Z);

    /// <summary>
    ///     原生 RaycastParams.Algorithm 的算法位标志
    /// </summary>
    /// <remarks>
    ///     可通过按位或组合多个模式, 如 <c>RaycastMode.SweepSphere | RaycastMode.MaterialFilter</c><br />
    ///     <see cref="TrySweepSphere" /> 会自动追加 <see cref="SweepSphere" />, 普通 <see cref="TryRaycast" /> 默认使用
    ///     <see cref="Ray" />
    /// </remarks>
    [Flags]
    public enum RaycastMode
    {
        /// <summary>
        ///     标准射线检测, 不启用额外算法
        /// </summary>
        Ray = 0,

        /// <summary>
        ///     使用球体扫掠代替细射线, 适合近似角色体积或路径宽度
        /// </summary>
        SweepSphere = 1,

        /// <summary>
        ///     忽略法线接近竖直方向的水平面碰撞, 阈值由 <see cref="RaycastOptions.MaxPlaneNormalY" /> 控制
        /// </summary>
        IgnoreHorizontal = 2,

        /// <summary>
        ///     启用材质过滤, 过滤值来自 <see cref="RaycastOptions.MaterialMask" /> 和 <see cref="RaycastOptions.MaterialValue" />
        /// </summary>
        MaterialFilter = 4
    }

    /// <summary>
    ///     碰撞查询选项
    /// </summary>
    /// <remarks>
    ///     默认值表示使用层掩码 1, 标准射线, 不启用材质过滤<br />
    ///     传给各查询方法时可用对象初始化器设置需要的字段, 如 <c>new() { LayerMask = 3, Mode = RaycastMode.SweepSphere }</c><br />
    ///     <see cref="MaterialMask" /> 非 0 时会自动启用 <see cref="RaycastMode.MaterialFilter" />
    /// </remarks>
    public readonly struct RaycastOptions
    {
        /// <summary>
        ///     要检测的碰撞层掩码, 0 表示使用默认层 1
        /// </summary>
        /// <remarks>
        ///     原生检测中碰撞体层掩码与该值按位与为 0 时会被忽略<br />
        ///     不确定目标层时保持默认值
        /// </remarks>
        public ulong LayerMask { get; init; }

        /// <summary>
        ///     射线算法模式
        /// </summary>
        /// <remarks>
        ///     可组合 <see cref="RaycastMode.SweepSphere" />, <see cref="RaycastMode.IgnoreHorizontal" /> 和
        ///     <see cref="RaycastMode.MaterialFilter" /><br />
        ///     <see cref="TrySweepSphere" /> 会强制追加 SweepSphere, <see cref="MaterialMask" /> 非 0 时会自动追加 MaterialFilter
        /// </remarks>
        public RaycastMode Mode { get; init; }

        /// <summary>
        ///     忽略水平面算法使用的法线 Y 阈值
        /// </summary>
        /// <remarks>
        ///     仅在 <see cref="RaycastMode.IgnoreHorizontal" /> 启用时参与原生算法<br />
        ///     值越低越容易保留接近水平的面, 值越高越容易忽略接近水平的面
        /// </remarks>
        public float MaxPlaneNormalY { get; init; }

        /// <summary>
        ///     材质过滤掩码
        /// </summary>
        /// <remarks>
        ///     非 0 时自动启用材质过滤<br />
        ///     <see cref="MaterialValue" /> 为 0 时, 原生逻辑匹配 <c>(colliderMaterial &amp; MaterialMask) != 0</c><br />
        ///     <see cref="MaterialValue" /> 非 0 时, 原生逻辑匹配 <c>(colliderMaterial &amp; MaterialMask) == MaterialValue</c>
        /// </remarks>
        public ulong MaterialMask { get; init; }

        /// <summary>
        ///     材质过滤目标值
        /// </summary>
        /// <remarks>
        ///     与 <see cref="MaterialMask" /> 配合使用, 控制材质过滤是按任意位命中还是精确匹配
        /// </remarks>
        public ulong MaterialValue { get; init; }

        /// <summary>
        ///     Mesh 碰撞体对象材质覆盖掩码
        /// </summary>
        /// <remarks>
        ///     仅原生 Mesh 材质计算使用, 用于替换三角形材质中的指定比特位<br />
        ///     常规射线检测通常保持默认 0
        /// </remarks>
        public ulong ObjectMaterialMask { get; init; }

        /// <summary>
        ///     Mesh 碰撞体对象材质覆盖值
        /// </summary>
        /// <remarks>
        ///     与 <see cref="ObjectMaterialMask" /> 配合使用, 用于形成 Mesh 三角形的有效材质<br />
        ///     常规射线检测通常保持默认 0
        /// </remarks>
        public ulong ObjectMaterialValue { get; init; }

        /// <summary>
        ///     获取传给原生查询的最终层掩码
        /// </summary>
        /// <returns><see cref="LayerMask" /> 为 0 时返回默认层 1, 否则返回 <see cref="LayerMask" /></returns>
        internal ulong GetLayerMask() =>
            LayerMask == 0 ? DEFAULT_LAYER_MASK : LayerMask;

        /// <summary>
        ///     获取传给原生查询的最终算法模式
        /// </summary>
        /// <returns><see cref="MaterialMask" /> 非 0 时追加材质过滤, 否则移除无效材质过滤标志后的模式</returns>
        internal RaycastMode GetMode() =>
            MaterialMask == 0 ? Mode & ~RaycastMode.MaterialFilter : Mode | RaycastMode.MaterialFilter;
    }

    #region 常量

    private const float DEFAULT_CEILING_DISTANCE   = 1000f;
    private const float DEFAULT_GROUND_ORIGIN_LIFT = 0.5f;
    private const float DEFAULT_MAX_DISTANCE       = 1000f;
    private const float DEFAULT_SAFE_CLEARANCE     = 1.5f;
    private const float MIN_RAY_DISTANCE           = 0.001f;
    private const float MIN_RAY_DISTANCE_SQUARED   = MIN_RAY_DISTANCE * MIN_RAY_DISTANCE;
    private const ulong DEFAULT_LAYER_MASK         = 1;

    #endregion
}

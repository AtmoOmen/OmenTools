using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Text.ReadOnly;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Helpers;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     提供在游戏内用 ImGui 标记某个物体或地点的轻量封装
///     支持临时注册 (区域切换自动清空) 与永久注册 (进入对应区域才激活)
/// </summary>
public sealed unsafe class ZoneIndicatorRenderer : OmenServiceBase<ZoneIndicatorRenderer>
{
    // 全局唯一且单调递增的句柄 ID, 永不复用
    private long nextID;

    // 句柄操作的唯一查找源, 永久注册常驻, 临时注册在区域切换时整批移除
    private readonly ConcurrentDictionary<ulong, ZoneIndicatorEntry> masterStore = [];

    // 当前区域应绘制的条目快照
    private ImmutableArray<ZoneIndicatorEntry> activeEntries = [];

    // Update 阶段预计算后的绘制状态, Draw 阶段只读消费, 原子替换
    private CachedEntryState[] cachedDrawStates = [];

    // Update 阶段复用缓冲区
    private readonly List<CachedEntryState> stateListBuffer  = [];
    private readonly List<CachedDrawTarget> targetListBuffer = [];
    private readonly List<TextDrawInfo>     textDrawBuffer   = [];

    // 文字尺寸缓存: (EntryID, TargetIndex) → 上一帧渲染尺寸, 用于居中对齐与背景绘制
    private readonly ConcurrentDictionary<(ulong EntryID, int TargetIndex), Vector2> textSizeCache = [];

    private static readonly Vector2 DefaultTextSize = new(50f, 20f);

    protected override void Init()
    {
        DService.Instance().ClientState.TerritoryChanged += OnTerritoryChanged;
        FrameworkManager.Instance().Reg(OnUpdate, 100);
        WindowManager.Instance().PostDraw += OnDraw;
    }

    protected override void Uninit()
    {
        DService.Instance().ClientState.TerritoryChanged -= OnTerritoryChanged;
        FrameworkManager.Instance().Unreg(OnUpdate);
        WindowManager.Instance().PostDraw -= OnDraw;

        masterStore.Clear();
        textSizeCache.Clear();
        activeEntries    = [];
        cachedDrawStates = [];
    }

    #region 注册

    /// <summary>
    ///     临时注册一个固定世界坐标标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegTemporary
    (
        Vector3                           position,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    ) =>
        Register(ZoneIndicatorEntry.ForPosition(NewID(), GameState.TerritoryType, false, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     临时注册一个跟随游戏物体的标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegTemporary
    (
        Func<List<nint>>                  objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return Register(ZoneIndicatorEntry.ForObject(NewID(), GameState.TerritoryType, false, objectGetter, objTextGetter, onDraw, options));
    }

    /// <summary>
    ///     临时注册一个跟随游戏物体的标记 (IGameObject 兼容路径), 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegTemporary
    (
        Func<List<IGameObject>>           objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return RegTemporary
        (
            AdaptObjectGetter(objectGetter),
            objTextGetter,
            onDraw,
            options
        );
    }

    /// <summary>
    ///     永久注册一个固定世界坐标标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegPermanent
    (
        uint                              territoryType,
        Vector3                           position,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    ) =>
        Register(ZoneIndicatorEntry.ForPosition(NewID(), territoryType, true, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     永久注册一个跟随游戏物体的标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegPermanent
    (
        uint                              territoryType,
        Func<List<nint>>                  objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return Register(ZoneIndicatorEntry.ForObject(NewID(), territoryType, true, objectGetter, objTextGetter, onDraw, options));
    }

    /// <summary>
    ///     永久注册一个跟随游戏物体的标记 (IGameObject 兼容路径), 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegPermanent
    (
        uint                              territoryType,
        Func<List<IGameObject>>           objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return RegPermanent
        (
            territoryType,
            AdaptObjectGetter(objectGetter),
            objTextGetter,
            onDraw,
            options
        );
    }

    private ZoneIndicatorHandle Register(ZoneIndicatorEntry entry)
    {
        masterStore[entry.ID] = entry;
        RebuildActiveEntries();

        return new() { ID = entry.ID };
    }

    private ulong NewID() =>
        (ulong)Interlocked.Increment(ref nextID);

    #endregion

    #region 句柄转发

    internal bool UnregisterByID(ulong id)
    {
        if (!masterStore.TryRemove(id, out _))
            return false;

        // 清理该条目关联的尺寸缓存
        foreach (var key in textSizeCache.Keys)
        {
            if (key.EntryID == id)
                textSizeCache.TryRemove(key, out _);
        }

        RebuildActiveEntries();
        return true;
    }

    internal bool UpdateByID(ulong id, Action<IZoneIndicatorMutable> mutator)
    {
        if (!masterStore.TryGetValue(id, out var entry))
            return false;

        mutator(entry);
        return true;
    }

    #endregion

    #region 内部

    private void OnTerritoryChanged(uint territoryType)
    {
        foreach (var (id, entry) in masterStore)
        {
            if (!entry.IsPermanent)
                masterStore.TryRemove(id, out _);
        }

        textSizeCache.Clear();
        RebuildActiveEntries();
    }

    private void RebuildActiveEntries()
    {
        var currentTerritory = GameState.TerritoryType;

        var builder = ImmutableArray.CreateBuilder<ZoneIndicatorEntry>();

        foreach (var entry in masterStore.Values)
        {
            if (entry.TerritoryType != currentTerritory)
                continue;

            builder.Add(entry);
        }

        activeEntries = builder.ToImmutable();
    }

    private void OnUpdate(IFramework framework)
    {
        var entries = activeEntries;

        if (entries.IsDefaultOrEmpty)
        {
            cachedDrawStates = [];
            return;
        }

        if (DService.Instance().ObjectTable.LocalPlayer is not { } localPlayer)
        {
            cachedDrawStates = [];
            return;
        }

        var playerPosition = localPlayer.Position;

        // 遮挡检测以摄像机视角为准, 取不到时回退玩家位置以避免误剔除
        var hasCameraPosition = false;
        var cameraPosition    = playerPosition;

        stateListBuffer.Clear();

        foreach (var entry in entries)
        {
            targetListBuffer.Clear();

            foreach (var (worldPosition, objectPtr) in entry.ResolveTargets())
            {
                var distanceSquared = Vector3.DistanceSquared(playerPosition, worldPosition);
                if (distanceSquared > entry.RenderRadiusSquared)
                    continue;

                if (entry.HiddenWhenBlocked)
                {
                    if (!hasCameraPosition)
                    {
                        if (!TryGetCameraPosition(out cameraPosition))
                            cameraPosition = playerPosition;
                        hasCameraPosition = true;
                    }

                    if (!RaycastHelper.HasLineOfSight(cameraPosition, worldPosition))
                        continue;
                }

                var textInfo      = ResolveText(entry, objectPtr, worldPosition);
                var resolvedColor = Vector4.Zero;
                if (textInfo is { Text: { } text })
                    resolvedColor = textInfo.TextColor ?? GetStableColor(text);
                targetListBuffer.Add(new CachedDrawTarget(worldPosition, MathF.Sqrt(distanceSquared), textInfo, resolvedColor));
            }

            if (targetListBuffer.Count > 0)
                stateListBuffer.Add(new CachedEntryState(entry.ID, entry.OnDraw, targetListBuffer.ToArray(), entry.Surrounding));
        }

        cachedDrawStates = stateListBuffer.ToArray();
    }

    private void OnDraw()
    {
        var states = cachedDrawStates;
        if (states.Length == 0)
            return;

        var bgDrawList = ImGui.GetBackgroundDrawList();
        var fgDrawList = ImGui.GetForegroundDrawList();

        // 第一遍: 绘制背景矩形、包围形状, 收集需要渲染文字的条目
        textDrawBuffer.Clear();

        foreach (var state in states)
        {
            var onDraw = state.OnDraw;

            for (var i = 0; i < state.Targets.Length; i++)
            {
                var target     = state.Targets[i];
                var isOnScreen = DService.Instance().GameGUI.WorldToScreen(target.WorldPosition, out var screenPosition);

                try
                {
                    var textSize = DefaultTextSize;
                    var cacheKey = (state.EntryID, i);

                    if (target.Text is { Text: { } text } textInfo && isOnScreen)
                    {
                        var finalScreenPos = screenPosition;
                        if (textInfo.TextOffset is { } offset)
                            finalScreenPos += new Vector2(offset.X, offset.Y);

                        var cachedSize = textSizeCache.GetValueOrDefault(cacheKey, DefaultTextSize);

                        DrawTextBackground(bgDrawList, finalScreenPos, target.TextColor, cachedSize);

                        textDrawBuffer.Add
                        (
                            new TextDrawInfo
                            {
                                ScreenPos  = finalScreenPos,
                                Text       = text,
                                Color      = target.TextColor,
                                Scale      = textInfo.TextScale ?? 1f,
                                CacheKey   = cacheKey,
                                CachedSize = cachedSize
                            }
                        );

                        textSize = cachedSize;
                    }

                    if (state.Surrounding is { } surrounding)
                        DrawSurrounding(bgDrawList, target.WorldPosition, surrounding);

                    if (onDraw != null)
                    {
                        var context = new ZoneIndicatorDrawContext
                        (
                            target.WorldPosition,
                            screenPosition,
                            isOnScreen,
                            target.Distance,
                            fgDrawList,
                            textSize
                        );
                        onDraw(context);
                    }
                }
                catch (Exception ex)
                {
                    DLog.Error("绘制区域物体标记时发生错误", ex);
                }
            }
        }

        // 第二遍: 单个全屏透明窗口承载所有 SeStringWrapped
        if (textDrawBuffer.Count > 0)
            RenderSeStringBatch();
    }

    private static ZoneIndicatorText? ResolveText
    (
        ZoneIndicatorEntry entry,
        nint               objectPtr,
        Vector3            worldPosition
    ) =>
        objectPtr != nint.Zero ? entry.ObjTextGetter?.Invoke(objectPtr) : entry.PosTextGetter?.Invoke(worldPosition);

    private static void DrawTextBackground
    (
        ImDrawListPtr drawList,
        Vector2       screenPosition,
        Vector4       textColor,
        Vector2       cachedSize
    )
    {
        const float ROUNDING = 5f;

        var bgHalfSize = cachedSize * 0.5f;
        var rectMin    = screenPosition              - bgHalfSize - LabelPadding;
        var rectMax    = screenPosition - bgHalfSize + cachedSize + LabelPadding;

        var bgCol     = new Vector4(textColor.X * 0.08f, textColor.Y * 0.08f, textColor.Z * 0.08f, 0.9f);
        var borderCol = textColor with { W = 0.8f };

        drawList.AddRectFilled(rectMin, rectMax, bgCol.ToUInt(), ROUNDING);
        drawList.AddRect(rectMin, rectMax, borderCol.ToUInt(), ROUNDING, ImDrawFlags.None, 1f);
    }

    private void RenderSeStringBatch()
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowSize(io.DisplaySize, ImGuiCond.Always);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,    Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);

        const ImGuiWindowFlags FLAGS = ImGuiWindowFlags.NoInputs              |
                                       ImGuiWindowFlags.NoDecoration          |
                                       ImGuiWindowFlags.NoBackground          |
                                       ImGuiWindowFlags.NoSavedSettings       |
                                       ImGuiWindowFlags.NoMove                |
                                       ImGuiWindowFlags.NoResize              |
                                       ImGuiWindowFlags.NoFocusOnAppearing    |
                                       ImGuiWindowFlags.NoBringToFrontOnFocus |
                                       ImGuiWindowFlags.NoNav                 |
                                       ImGuiWindowFlags.NoDocking;

        if (ImGui.Begin("##zone_texts", FLAGS))
        {
            foreach (var d in textDrawBuffer)
            {
                var pos = d.ScreenPos - (d.CachedSize * 0.5f);
                ImGui.SetCursorScreenPos(pos);

                using (FontManager.Instance().GetUIFont(d.Scale).Push())
                using (ImRaii.PushColor(ImGuiCol.Text, d.Color.ToUInt()))
                using (ImRaii.Group())
                    ImGuiHelpers.SeStringWrapped(d.Text);

                var newSize = ImGui.GetItemRectSize();
                if (newSize is { X: > 0, Y: > 0 })
                    textSizeCache[d.CacheKey] = newSize;
            }

            ImGui.End();
        }

        ImGui.PopStyleVar(2);
    }

    private static void DrawSurrounding(ImDrawListPtr drawList, Vector3 worldPos, ZoneIndicatorSurrounding s)
    {
        var color = s.Color.ToUInt();
        var gui   = DService.Instance().GameGUI;

        // 复用预计算的单位形状偏移, 在绘制时按半径缩放, 避免每帧堆分配与三角函数计算
        var unitOffsets = s.Type switch
        {
            ZoneIndicatorSurrounding.Shape.Circle   => UnitCircleOffsets,
            ZoneIndicatorSurrounding.Shape.Square   => UnitSquareOffsets,
            ZoneIndicatorSurrounding.Shape.Triangle => UnitTriangleOffsets,
            _                                       => null
        };

        if (unitOffsets == null)
            return;

        DrawPolyline(drawList, gui, worldPos, unitOffsets, s.Radius, color, s.Thickness);
    }

    private static void DrawPolyline
    (
        ImDrawListPtr         drawList,
        IGameGui              gui,
        Vector3               center,
        ReadOnlySpan<Vector2> unitOffsets,
        float                 radius,
        uint                  color,
        float                 thickness
    )
    {
        var n = unitOffsets.Length;

        // 预计算所有世界坐标顶点, 单位偏移按半径缩放
        Span<Vector3> worldVerts = stackalloc Vector3[n];
        for (var i = 0; i < n; i++)
            worldVerts[i] = new Vector3(center.X + (unitOffsets[i].X * radius), center.Y, center.Z + (unitOffsets[i].Y * radius));

        // 投影所有顶点
        Span<Vector2?> screenVerts = stackalloc Vector2?[n];
        for (var i = 0; i < n; i++)
            if (gui.WorldToScreen(worldVerts[i], out var sp))
                screenVerts[i] = sp;

        // 逐边绘制, 单端在摄像机后方时裁剪到近平面
        for (var i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            DrawClippedEdge(drawList, gui, worldVerts[i], worldVerts[j], screenVerts[i], screenVerts[j], color, thickness);
        }
    }

    private static void DrawClippedEdge
    (
        ImDrawListPtr drawList,
        IGameGui      gui,
        Vector3       worldA,
        Vector3       worldB,
        Vector2?      screenA,
        Vector2?      screenB,
        uint          color,
        float         thickness
    )
    {
        switch (screenA)
        {
            // 两端都可见 — 直接画
            case { } pa when screenB is { } pb:
                drawList.AddLine(pa, pb, color, thickness);
                return;
            // 两端都在摄像机后 — 跳过
            case null when screenB is null:
                return;
        }

        // 一端可见、一端在摄像机后 — 二分裁剪到近平面
        var visibleWorld  = screenA is not null ? worldA : worldB;
        var behindWorld   = screenA is null ? worldA : worldB;
        var visibleScreen = screenA ?? screenB!.Value;

        if (TryClipToNearPlane(gui, visibleWorld, behindWorld, out var clipScreen))
            drawList.AddLine(visibleScreen, clipScreen, color, thickness);
    }

    private static bool TryClipToNearPlane(IGameGui gui, Vector3 visible, Vector3 behind, out Vector2 clipScreen)
    {
        clipScreen = default;

        var lo = 0f;
        var hi = 1f;

        // 二分查找近平面交点
        for (var i = 0; i < 8; i++)
        {
            var mid = (lo + hi) * 0.5f;
            var wp = new Vector3
            (
                visible.X + ((behind.X - visible.X) * mid),
                visible.Y + ((behind.Y - visible.Y) * mid),
                visible.Z + ((behind.Z - visible.Z) * mid)
            );

            if (gui.WorldToScreen(wp, out _))
                lo = mid;
            else
                hi = mid;
        }

        var clipWorld = new Vector3
        (
            visible.X + ((behind.X - visible.X) * lo),
            visible.Y + ((behind.Y - visible.Y) * lo),
            visible.Z + ((behind.Z - visible.Z) * lo)
        );

        return gui.WorldToScreen(clipWorld, out clipScreen);
    }

    private static int GetDeterministicHashCode(ReadOnlySeString text)
    {
        var str = text.ToString();

        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i + 1 < str.Length)
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    private static Vector4 GetStableColor(ReadOnlySeString text)
    {
        var hash = GetDeterministicHashCode(text);
        var hue  = (hash & 0x7FFFFFFF) % 360 / 360f;
        return HslToRgb(hue, 0.8f, 0.6f);
    }

    private static Vector4 HslToRgb(float h, float s, float l)
    {
        float r, g, b;

        if (s == 0)
            r = g = b = l;
        else
        {
            var q = l < 0.5f ? l * (1f + s) : l + s - (l * s);
            var p = (2f * l) - q;
            r = HueToRgb(p, q, h + (1f / 3f));
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - (1f / 3f));
        }

        return new Vector4(r, g, b, 1.0f);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f)
            t += 1f;
        if (t > 1f)
            t -= 1f;

        return t switch
        {
            < 1f / 6f => p + ((q - p) * 6f * t),
            < 1f / 2f => q,
            < 2f / 3f => p + ((q - p) * ((2f / 3f) - t) * 6f),
            _         => p
        };
    }

    private static Func<List<nint>> AdaptObjectGetter(Func<List<IGameObject>> getter) =>
        () =>
        {
            var objects = getter();
            if (objects is not { Count: > 0 })
                return [];

            var result = new List<nint>(objects.Count);

            foreach (var go in objects)
            {
                if (go.Address != nint.Zero)
                    result.Add((nint)go.ToStruct());
            }

            return result;
        };

    private static bool TryGetCameraPosition(out Vector3 position)
    {
        position = default;

        var manager = CameraManager.Instance();
        if (manager == null)
            return false;

        var camera = manager->Camera;
        if (camera == null)
            return false;

        position = camera->CameraBase.SceneCamera.Position;
        return true;
    }

    #endregion

    #region 常量

    // 名牌背景内边距
    private static readonly Vector2 LabelPadding = new(6f, 3f);

    // 圆形描边分段数
    private const int CIRCLE_SEGMENTS = 64;

    // 单位半径形状的 XZ 偏移, 绘制时按实际半径缩放, 避免每帧重新计算与堆分配
    private static readonly Vector2[] UnitCircleOffsets   = BuildUnitCircleOffsets();
    private static readonly Vector2[] UnitSquareOffsets   = [new(-1f, -1f), new(1f, -1f), new(1f, 1f), new(-1f, 1f)];
    private static readonly Vector2[] UnitTriangleOffsets = [new(0f, -1f), new(0.8660254f, 0.5f), new(-0.8660254f, 0.5f)];

    private static Vector2[] BuildUnitCircleOffsets()
    {
        var offsets = new Vector2[CIRCLE_SEGMENTS];

        for (var i = 0; i < CIRCLE_SEGMENTS; i++)
        {
            var angle = MathF.Tau * i / CIRCLE_SEGMENTS;
            offsets[i] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        return offsets;
    }

    #endregion

    #region 嵌套类

    internal sealed class CachedEntryState
    {
        internal CachedEntryState(ulong entryID, Action<ZoneIndicatorDrawContext>? onDraw, CachedDrawTarget[] targets, ZoneIndicatorSurrounding? surrounding)
        {
            EntryID     = entryID;
            OnDraw      = onDraw;
            Targets     = targets;
            Surrounding = surrounding;
        }

        /// <summary>条目 ID, 用于尺寸缓存索引</summary>
        public readonly ulong EntryID;

        /// <summary>自定义绘制委托, null 表示仅绘制文字</summary>
        public readonly Action<ZoneIndicatorDrawContext>? OnDraw;

        /// <summary>经距离剔除与遮挡剔除后的绘制目标</summary>
        public readonly CachedDrawTarget[] Targets;

        /// <summary>包围形状参数, null 表示不绘制形状</summary>
        public readonly ZoneIndicatorSurrounding? Surrounding;
    }

    internal readonly struct CachedDrawTarget
    {
        internal CachedDrawTarget(Vector3 worldPosition, float distance, ZoneIndicatorText? text, Vector4 textColor)
        {
            WorldPosition = worldPosition;
            Distance      = distance;
            Text          = text;
            TextColor     = textColor;
        }

        /// <summary>目标世界坐标</summary>
        public readonly Vector3 WorldPosition;

        /// <summary>目标到玩家的距离 (yalm), 在 Update 阶段计算</summary>
        public readonly float Distance;

        /// <summary>已解析的文字参数, null 表示不绘制文字</summary>
        public readonly ZoneIndicatorText? Text;

        /// <summary>已解析并缓存的颜色</summary>
        public readonly Vector4 TextColor;
    }

    /// <summary>Draw 阶段第一遍收集的文字绘制信息, 供第二遍批量 SeString 渲染</summary>
    internal struct TextDrawInfo
    {
        public Vector2          ScreenPos;
        public ReadOnlySeString Text;
        public Vector4          Color;
        public float            Scale;
        public (ulong, int)     CacheKey;
        public Vector2          CachedSize;
    }

    #endregion
}

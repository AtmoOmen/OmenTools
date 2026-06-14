using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Text.ReadOnly;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Helpers;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService.ZoneIndicator;

public sealed unsafe class ZoneIndicatorRenderer : OmenServiceBase<ZoneIndicatorRenderer>
{
    #region 公开

    /// <summary>
    ///     临时注册一个或多个世界坐标标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegTemporary
    (
        Func<List<Vector3>>               positionGetter,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(positionGetter);
        return Register(ZoneIndicatorEntry.ForPosition(NewID(), GameState.TerritoryType, false, positionGetter, posTextGetter, onDraw, options));
    }

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
    ///     永久注册一个或多个世界坐标标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegPermanent
    (
        uint                              territoryType,
        Func<List<Vector3>>               positionGetter,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(positionGetter);
        return Register(ZoneIndicatorEntry.ForPosition(NewID(), territoryType, true, positionGetter, posTextGetter, onDraw, options));
    }

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
    #endregion

    private readonly ConcurrentDictionary<ulong, ZoneIndicatorEntry> masterStore = [];

    private ImmutableArray<ZoneIndicatorEntry> activeEntries = [];

    private CachedEntryState[] cachedDrawStates = [];

    private readonly List<CachedEntryState> stateListBuffer  = [];
    private readonly List<CachedDrawTarget> targetListBuffer = [];
    private readonly List<TextDrawInfo>     textDrawBuffer   = [];

    private readonly ConcurrentDictionary<(ulong EntryID, int TargetIndex), (Vector2 Size, float TextOnlyHeight)> textSizeCache = [];

    private long nextID;

    #region 生命周期

    protected override void Init()
    {
        DService.Instance().ClientState.TerritoryChanged += OnZoneChanged;
        FrameworkManager.Instance().Reg(OnUpdate, 100);
        WindowManager.Instance().PostDraw += OnDraw;
    }

    protected override void Uninit()
    {
        DService.Instance().ClientState.TerritoryChanged -= OnZoneChanged;
        FrameworkManager.Instance().Unreg(OnUpdate);
        WindowManager.Instance().PostDraw -= OnDraw;

        masterStore.Clear();
        textSizeCache.Clear();
        activeEntries    = [];
        cachedDrawStates = [];
    }

    #endregion

    #region 事件

    private void OnZoneChanged(uint territoryType)
    {
        foreach (var (id, entry) in masterStore)
        {
            if (!entry.IsPermanent)
                masterStore.TryRemove(id, out _);
        }

        textSizeCache.Clear();
        RebuildActiveEntries();
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

        var      playerPosition = localPlayer.Position;
        Vector3? cameraPos      = null;

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
                    cameraPos ??= TryGetCameraPosition(out var cp) ? cp : playerPosition;
                    if (!RaycastHelper.HasLineOfSight(cameraPos.Value, worldPosition))
                        continue;
                }

                var textInfo = ResolveText(entry, objectPtr, worldPosition);
                var resolvedColor = textInfo switch
                {
                    { Text: { } text }  => textInfo.TextColor ?? GetStableColor(text),
                    { Image: not null } => textInfo.TextColor ?? new Vector4(1f),
                    _                   => Vector4.Zero
                };
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
                    var textSize = FallbackTextSize;
                    var cacheKey = (state.EntryID, i);

                    if (target.Text is { } textInfo && isOnScreen && (textInfo.Text is not null || textInfo.Image is not null))
                    {
                        var finalScreenPos = textInfo.TextOffset is { } offset
                                                 ? screenPosition + new Vector2(offset.X, offset.Y)
                                                 : screenPosition;

                        var cachedSize = textSizeCache.TryGetValue(cacheKey, out var c) ? c.Size : FallbackTextSize;

                        DrawTextBackground(bgDrawList, finalScreenPos, target.TextColor, cachedSize);

                        textDrawBuffer.Add
                        (
                            new TextDrawInfo
                            {
                                ScreenPos  = finalScreenPos,
                                Text       = textInfo.Text,
                                Color      = target.TextColor,
                                Scale      = textInfo.TextScale ?? 1f,
                                CacheKey   = cacheKey,
                                CachedSize = cachedSize,
                                Image      = textInfo.Image
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

        if (textDrawBuffer.Count > 0)
            RenderSeStringBatch();
    }

    #endregion

    private ZoneIndicatorHandle Register(ZoneIndicatorEntry entry)
    {
        masterStore[entry.ID] = entry;
        RebuildActiveEntries();
        return new() { ID = entry.ID };
    }

    private ulong NewID() =>
        (ulong)Interlocked.Increment(ref nextID);

    internal bool UnregisterByID(ulong id)
    {
        if (!masterStore.TryRemove(id, out _))
            return false;

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

    private void RebuildActiveEntries()
    {
        var currentTerritory = GameState.TerritoryType;

        var builder = ImmutableArray.CreateBuilder<ZoneIndicatorEntry>();

        foreach (var entry in masterStore.Values)
        {
            if (entry.TerritoryType == currentTerritory)
                builder.Add(entry);
        }

        activeEntries = builder.ToImmutable();
    }


    private static ZoneIndicatorText? ResolveText(ZoneIndicatorEntry entry, nint objectPtr, Vector3 worldPosition) =>
        objectPtr != nint.Zero ? entry.ObjTextGetter?.Invoke(objectPtr) : entry.PosTextGetter?.Invoke(worldPosition);

    private static void DrawTextBackground(ImDrawListPtr drawList, Vector2 screenPosition, Vector4 textColor, Vector2 cachedSize)
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
                RenderTextItem(d);

            ImGui.End();
        }

        ImGui.PopStyleVar(2);
    }

    private void RenderTextItem(TextDrawInfo d)
    {
        var pos = d.ScreenPos - (d.CachedSize * 0.5f);
        ImGui.SetCursorScreenPos(pos);

        using (FontManager.Instance().GetUIFont(d.Scale).Push())
        using (ImRaii.PushColor(ImGuiCol.Text, d.Color.ToUInt()))
        {
            var textOnlyHeight = 0f;

            using (ImRaii.Group())
            {
                switch (d.Image, d.Text)
                {
                    case ({ } image, { } text):
                    {
                        var (_, cachedTextOnlyH) = textSizeCache.TryGetValue(d.CacheKey, out var c) ? c : (FallbackTextSize, 0f);
                        var rowOriginY = ImGui.GetCursorPosY();
                        var imageSize  = image.Size();
                        var textHeight = cachedTextOnlyH > 0f ? cachedTextOnlyH : ImGui.GetTextLineHeight();
                        var rowHeight  = Math.Max(imageSize.Y, textHeight);

                        if (image.Texture.GetWrapOrDefault() is { } imageWrap && imageWrap.Handle != nint.Zero)
                        {
                            ImGui.SetCursorPosY(rowOriginY + ((rowHeight - imageSize.Y) * 0.5f));
                            ImGui.Image(imageWrap.Handle, imageSize);
                            ImGui.SameLine();
                        }

                        ImGui.SetCursorPosY(rowOriginY + ((rowHeight - textHeight) * 0.5f));
                        ImGuiHelpers.SeStringWrapped(text);
                        textOnlyHeight = ImGui.GetItemRectSize().Y;
                        break;
                    }
                    case ({ } imageOnly, null):
                    {
                        if (imageOnly.Texture.GetWrapOrDefault() is { } wrap && wrap.Handle != nint.Zero)
                            ImGui.Image(wrap.Handle, imageOnly.Size());
                        break;
                    }
                    case (null, { } textOnly):
                        ImGuiHelpers.SeStringWrapped(textOnly);
                        textOnlyHeight = ImGui.GetItemRectSize().Y;
                        break;
                }
            }

            var newSize = ImGui.GetItemRectSize();
            if (newSize is { X: > 0, Y: > 0 })
                textSizeCache[d.CacheKey] = (newSize, textOnlyHeight);
        }
    }


    private static void DrawSurrounding(ImDrawListPtr drawList, Vector3 worldPos, ZoneIndicatorSurrounding s)
    {
        var gui   = DService.Instance().GameGUI;
        var color = s.Color.ToUInt();

        var unitOffsets = s.Type switch
        {
            ZoneIndicatorSurrounding.Shape.Circle   => UnitCircleOffsets,
            ZoneIndicatorSurrounding.Shape.Square   => UnitSquareOffsets,
            ZoneIndicatorSurrounding.Shape.Triangle => UnitTriangleOffsets,
            _                                       => null
        };

        if (unitOffsets is not null)
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

        Span<Vector3> worldVerts = stackalloc Vector3[n];
        for (var i = 0; i < n; i++)
            worldVerts[i] = new Vector3(center.X + (unitOffsets[i].X * radius), center.Y, center.Z + (unitOffsets[i].Y * radius));

        Span<Vector2?> screenVerts = stackalloc Vector2?[n];
        for (var i = 0; i < n; i++)
            if (gui.WorldToScreen(worldVerts[i], out var sp))
                screenVerts[i] = sp;

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
            case { } pa when screenB is { } pb:
                drawList.AddLine(pa, pb, color, thickness);
                return;
            case null when screenB is null:
                return;
        }

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

        for (var i = 0; i < 8; i++)
        {
            var mid = (lo + hi) * 0.5f;
            if (gui.WorldToScreen(Vector3.Lerp(visible, behind, mid), out _))
                lo = mid;
            else
                hi = mid;
        }

        return gui.WorldToScreen(Vector3.Lerp(visible, behind, lo), out clipScreen);
    }


    private static Vector4 GetStableColor(ReadOnlySeString text)
    {
        var hue = (GetStableHash(text) & 0x7FFFFFFF) % 360 / 360f;
        return HslToRgb(hue, 0.8f, 0.6f);
    }

    private static int GetStableHash(ReadOnlySeString text)
    {
        var str = text.ToString();

        unchecked
        {
            var h1 = (5381 << 16) + 5381;
            var h2 = h1;

            for (var i = 0; i < str.Length; i += 2)
            {
                h1 = ((h1 << 5) + h1) ^ str[i];
                if (i + 1 < str.Length)
                    h2 = ((h2 << 5) + h2) ^ str[i + 1];
            }

            return h1 + (h2 * 1566083941);
        }
    }

    private static Vector4 HslToRgb(float h, float s, float l)
    {
        if (s == 0)
            return new(l, l, l, 1);

        var q = l < 0.5f ? l * (1 + s) : l + s - (l * s);
        var p = (2 * l) - q;

        return new(HueToRgb(p, q, h + (1f / 3f)), HueToRgb(p, q, h), HueToRgb(p, q, h - (1f / 3f)), 1);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;

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

    #region 常量

    private static readonly Vector2 LabelPadding = new(6f, 3f);

    private const int CIRCLE_SEGMENTS = 64;

    private static readonly Vector2[] UnitCircleOffsets   = BuildUnitCircleOffsets();
    private static readonly Vector2[] UnitSquareOffsets   = [new(-1f, -1f), new(1f, -1f), new(1f, 1f), new(-1f, 1f)];
    private static readonly Vector2[] UnitTriangleOffsets = [new(0f, -1f), new(0.8660254f, 0.5f), new(-0.8660254f, 0.5f)];

    private static readonly Vector2 FallbackTextSize = new(50f, 20f);

    private static Vector2[] BuildUnitCircleOffsets()
    {
        var offsets = new Vector2[CIRCLE_SEGMENTS];

        for (var i = 0; i < CIRCLE_SEGMENTS; i++)
        {
            var angle = MathF.Tau * i / CIRCLE_SEGMENTS;
            offsets[i] = new(MathF.Cos(angle), MathF.Sin(angle));
        }

        return offsets;
    }

    #endregion

    #region 嵌套类

    internal sealed class CachedEntryState
    (
        ulong                             entryID,
        Action<ZoneIndicatorDrawContext>? onDraw,
        CachedDrawTarget[]                targets,
        ZoneIndicatorSurrounding?         surrounding
    )
    {
        /// <summary>条目 ID, 用于尺寸缓存索引</summary>
        public ulong EntryID { get; } = entryID;

        /// <summary>自定义绘制委托, null 表示仅绘制文字</summary>
        public Action<ZoneIndicatorDrawContext>? OnDraw { get; } = onDraw;

        /// <summary>经距离剔除与遮挡剔除后的绘制目标</summary>
        public CachedDrawTarget[] Targets { get; } = targets;

        /// <summary>包围形状参数, null 表示不绘制形状</summary>
        public ZoneIndicatorSurrounding? Surrounding { get; } = surrounding;
    }

    internal readonly struct CachedDrawTarget
    (
        Vector3            worldPosition,
        float              distance,
        ZoneIndicatorText? text,
        Vector4            textColor
    )
    {
        /// <summary>目标世界坐标</summary>
        public Vector3 WorldPosition { get; } = worldPosition;

        /// <summary>目标到玩家的距离 (yalm), 在 Update 阶段计算</summary>
        public float Distance { get; } = distance;

        /// <summary>已解析的文字参数, null 表示不绘制文字</summary>
        public ZoneIndicatorText? Text { get; } = text;

        /// <summary>已解析并缓存的颜色</summary>
        public Vector4 TextColor { get; } = textColor;
    }

    /// <summary>Draw 阶段第一遍收集的文字绘制信息, 供第二遍批量 SeString 渲染</summary>
    internal struct TextDrawInfo
    {
        public Vector2                      ScreenPos;
        public ReadOnlySeString?            Text;
        public Vector4                      Color;
        public float                        Scale;
        public (ulong, int)                 CacheKey;
        public Vector2                      CachedSize;
        public ZoneIndicatorText.TextImage? Image;
    }

    #endregion
}

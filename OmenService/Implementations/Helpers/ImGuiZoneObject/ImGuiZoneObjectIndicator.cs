using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Helpers;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     提供在游戏内用 ImGui 标记某个物体或地点的轻量封装
///     支持临时注册 (区域切换自动清空) 与永久注册 (进入对应区域才激活)
/// </summary>
public sealed unsafe class ImGuiZoneObjectIndicator : OmenServiceBase<ImGuiZoneObjectIndicator>
{
    // 全局唯一且单调递增的句柄 ID, 永不复用
    private long nextID;

    // 句柄操作的唯一查找源, 永久注册常驻, 临时注册在区域切换时整批移除
    private readonly ConcurrentDictionary<ulong, IndicatorEntry> masterStore = [];

    // 当前区域应绘制的条目快照
    private ImmutableArray<IndicatorEntry> activeEntries = [];

    // Update 阶段预计算后的绘制状态, Draw 阶段只读消费, 原子替换
    private CachedEntryState[] cachedDrawStates = [];

    // Update 阶段复用缓冲区
    private readonly List<CachedEntryState> stateListBuffer  = [];
    private readonly List<CachedDrawTarget> targetListBuffer = [];

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
        activeEntries    = [];
        cachedDrawStates = [];
    }

    #region 注册

    /// <summary>
    ///     临时注册一个固定世界坐标标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegisterTemporary
    (
        Vector3                           position,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    ) =>
        Register(IndicatorEntry.ForPosition(NewID(), GameState.TerritoryType, false, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     临时注册一个跟随游戏物体的标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegisterTemporary
    (
        Func<List<IGameObject>>               objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                 options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return Register(IndicatorEntry.ForObject(NewID(), GameState.TerritoryType, false, objectGetter, objTextGetter, onDraw, options));
    }

    /// <summary>
    ///     永久注册一个固定世界坐标标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegisterPermanent
    (
        uint                              territoryType,
        Vector3                           position,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    ) =>
        Register(IndicatorEntry.ForPosition(NewID(), territoryType, true, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     永久注册一个跟随游戏物体的标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegisterPermanent
    (
        uint                                  territoryType,
        Func<List<IGameObject>>               objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                 options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(objectGetter);
        return Register(IndicatorEntry.ForObject(NewID(), territoryType, true, objectGetter, objTextGetter, onDraw, options));
    }

    private ZoneIndicatorHandle Register(IndicatorEntry entry)
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

        RebuildActiveEntries();
        return true;
    }

    internal bool UpdateTextByID
    (
        ulong                                 id,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter,
        Func<Vector3, ZoneIndicatorText>?     posTextGetter
    )
    {
        if (!masterStore.TryGetValue(id, out var entry))
            return false;

        entry.ObjTextGetter = objTextGetter;
        entry.PosTextGetter = posTextGetter;
        return true;
    }

    internal bool UpdateDrawByID(ulong id, Action<ZoneIndicatorDrawContext>? onDraw)
    {
        if (!masterStore.TryGetValue(id, out var entry))
            return false;

        entry.OnDraw = onDraw;
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

        RebuildActiveEntries();
    }

    private void RebuildActiveEntries()
    {
        var currentTerritory = GameState.TerritoryType;

        var builder = ImmutableArray.CreateBuilder<IndicatorEntry>();

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

            foreach (var (worldPosition, gameObject) in entry.ResolveTargets())
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

                var textInfo      = ResolveText(entry, gameObject, worldPosition);
                var resolvedColor = Vector4.Zero;
                if (textInfo is { Text: { } text })
                    resolvedColor = textInfo.TextColor ?? GetStableColor(text);
                targetListBuffer.Add(new CachedDrawTarget(worldPosition, MathF.Sqrt(distanceSquared), textInfo, resolvedColor));
            }

            if (targetListBuffer.Count > 0)
                stateListBuffer.Add(new CachedEntryState(entry.OnDraw, targetListBuffer.ToArray()));
        }

        cachedDrawStates = stateListBuffer.ToArray();
    }

    private void OnDraw()
    {
        var states = cachedDrawStates;
        if (states.Length == 0)
            return;

        var drawList = ImGui.GetForegroundDrawList();

        foreach (var state in states)
        {
            var onDraw = state.OnDraw;

            foreach (var target in state.Targets)
            {
                var isOnScreen = DService.Instance().GameGUI.WorldToScreen(target.WorldPosition, out var screenPosition);

                try
                {
                    if (target.Text is { Text: { } text } textInfo && isOnScreen)
                    {
                        var finalScreenPos = screenPosition;
                        if (textInfo.TextOffset is { } offset)
                            finalScreenPos += new Vector2(offset.X, offset.Y);

                        DrawText(drawList, finalScreenPos, text, target.TextColor, textInfo.TextScale ?? 1f);
                    }

                    if (onDraw != null)
                    {
                        var context = new ZoneIndicatorDrawContext
                        (
                            target.WorldPosition,
                            screenPosition,
                            isOnScreen,
                            target.Distance,
                            drawList
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
    }

    private static ZoneIndicatorText? ResolveText
    (
        IndicatorEntry entry,
        IGameObject?   gameObject,
        Vector3        worldPosition
    ) =>
        gameObject != null ? entry.ObjTextGetter?.Invoke(gameObject) : entry.PosTextGetter?.Invoke(worldPosition);

    private static void DrawText
    (
        ImDrawListPtr drawList,
        Vector2       screenPosition,
        string        text,
        Vector4       textColor,
        float         textScale
    )
    {
        const float ROUNDING = 5f;

        using var font = FontManager.Instance().GetUIFont(textScale).Push();

        var textSize     = ImGui.CalcTextSize(text);
        var textPosition = screenPosition - (textSize * 0.5f);
        var rectMin      = textPosition   - LabelPadding;
        var rectMax      = textPosition   + textSize + LabelPadding;

        var bgCol     = new Vector4(textColor.X * 0.08f, textColor.Y * 0.08f, textColor.Z * 0.08f, 0.9f);
        var borderCol = textColor with { W = 0.8f };

        drawList.AddRectFilled(rectMin, rectMax, bgCol.ToUInt(), ROUNDING);
        drawList.AddRect(rectMin, rectMax, borderCol.ToUInt(), ROUNDING, ImDrawFlags.None, 1f);
        drawList.AddText(textPosition, textColor.ToUInt(), text);
    }

    private static int GetDeterministicHashCode(string str)
    {
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

    private static Vector4 GetStableColor(string text)
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

    #endregion
}

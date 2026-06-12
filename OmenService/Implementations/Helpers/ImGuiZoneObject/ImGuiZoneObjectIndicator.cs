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
    // 全局唯一且单调递增的句柄 ID, 永不复用, 保证失效句柄不会误命中后续注册
    private long nextID;

    // 句柄操作的唯一查找源, 永久注册常驻, 临时注册在区域切换时整批移除
    private readonly ConcurrentDictionary<ulong, IndicatorEntry> masterStore = [];

    // 当前区域应绘制的条目快照, 热循环只读此数组, 低频事件时重建
    private ImmutableArray<IndicatorEntry> activeEntries = [];

    protected override void Init()
    {
        DService.Instance().ClientState.TerritoryChanged += OnTerritoryChanged;
        WindowManager.Instance().PostDraw                += OnDraw;
    }

    protected override void Uninit()
    {
        DService.Instance().ClientState.TerritoryChanged -= OnTerritoryChanged;
        WindowManager.Instance().PostDraw                -= OnDraw;

        masterStore.Clear();
        activeEntries = [];
    }

    #region 注册

    /// <summary>
    ///     临时注册一个固定世界坐标标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegisterTemporary
    (
        Vector3                              position,
        Func<Vector3, ZoneIndicatorText>?     posTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                options       = null
    ) =>
        Register(IndicatorEntry.ForPosition(NewID(), GameState.TerritoryType, false, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     临时注册一个跟随游戏物体的标记, 区域切换时自动清空
    /// </summary>
    public ZoneIndicatorHandle RegisterTemporary
    (
        Func<List<IGameObject>>             objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                options       = null
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
        uint                                 territoryType,
        Vector3                              position,
        Func<Vector3, ZoneIndicatorText>?     posTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                options       = null
    ) =>
        Register(IndicatorEntry.ForPosition(NewID(), territoryType, true, position, posTextGetter, onDraw, options));

    /// <summary>
    ///     永久注册一个跟随游戏物体的标记, 进入对应区域才激活, 取消注册前一直保留
    /// </summary>
    public ZoneIndicatorHandle RegisterPermanent
    (
        uint                                 territoryType,
        Func<List<IGameObject>>             objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                options       = null
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

    // 以下方法供 ZoneIndicatorHandle 成员方法转发, 句柄已失效时静默返回 false

    internal bool UnregisterByID(ulong id)
    {
        if (!masterStore.TryRemove(id, out _))
            return false;

        RebuildActiveEntries();
        return true;
    }

    internal bool UpdateTextByID
    (
        ulong                                id,
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

    // 区域切换时移除全部临时注册, 并重建激活快照
    private void OnTerritoryChanged(uint territoryType)
    {
        foreach (var (id, entry) in masterStore)
        {
            if (!entry.IsPermanent)
                masterStore.TryRemove(id, out _);
        }

        RebuildActiveEntries();
    }

    // 仅在低频事件 (注册/取消/区域切换) 时调用, 计算出当前区域应绘制的子集
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

    private void OnDraw()
    {
        var entries = activeEntries;
        if (entries.IsDefaultOrEmpty)
            return;

        if (DService.Instance().ObjectTable.LocalPlayer is not { } localPlayer)
            return;

        var playerPosition = localPlayer.Position;
        var drawList       = ImGui.GetForegroundDrawList();

        // 遮挡检测以摄像机视角为准, 取不到时回退玩家位置以避免误剔除
        var hasCameraPosition = false;
        var cameraPosition    = playerPosition;

        foreach (var entry in entries)
        {
            var onDraw = entry.OnDraw;

            foreach (var (worldPosition, gameObject) in entry.ResolveTargets())
            {
                var distanceSquared = Vector3.DistanceSquared(playerPosition, worldPosition);
                var renderRadius    = entry.RenderRadius;
                if (distanceSquared > renderRadius * renderRadius)
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

                var isOnScreen = DService.Instance().GameGUI.WorldToScreen(worldPosition, out var screenPosition);

                var context = new ZoneIndicatorDrawContext
                (
                    worldPosition,
                    screenPosition,
                    isOnScreen,
                    MathF.Sqrt(distanceSquared),
                    drawList
                );

                try
                {
                    var textInfo = ResolveText(entry, gameObject, worldPosition);

                    if (textInfo?.Text is { } text && isOnScreen)
                    {
                        var finalScreenPos = screenPosition;
                        if (textInfo.TextOffset is { } offset)
                            finalScreenPos += new Vector2(offset.X, offset.Y);

                        DrawText(drawList, finalScreenPos, text, textInfo.TextColor ?? DefaultTextColor, textInfo.TextScale ?? 1f);
                    }

                    onDraw?.Invoke(context);
                }
                catch (Exception ex)
                {
                    DLog.Error($"绘制区域物体标记时发生错误: ID {entry.ID}", ex);
                }
            }
        }
    }

    // 根据条目类型和当前目标解析文字参数
    private static ZoneIndicatorText? ResolveText
    (
        IndicatorEntry  entry,
        IGameObject?    gameObject,
        Vector3         worldPosition
    )
    {
        if (gameObject != null)
            return entry.ObjTextGetter?.Invoke(gameObject);

        return entry.PosTextGetter?.Invoke(worldPosition);
    }

    // 在屏幕坐标处绘制带名牌背景的文字
    private static void DrawText(ImDrawListPtr drawList, Vector2 screenPosition, string text, Vector4 textColor, float textScale)
    {
        var textSize     = ImGui.CalcTextSize(text) * textScale;
        var textPosition = screenPosition - (textSize                * 0.5f);
        var rectMin      = textPosition   - (LabelPadding            * textScale);
        var rectMax      = textPosition   + textSize + (LabelPadding * textScale);
        var rounding     = (rectMax.Y - rectMin.Y) * 0.25f;

        drawList.AddRectFilled(rectMin, rectMax, LabelBackgroundColor, rounding);
        drawList.AddRect(rectMin, rectMax, LabelBorderColor, rounding, ImDrawFlags.None, 1f);

        var font     = ImGui.GetFont();
        var fontSize = font.FontSize * textScale;
        
        drawList.AddText(font, fontSize, textPosition, textColor.ToUInt(), text);
    }

    // 获取当前激活摄像机的世界坐标
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

    internal static readonly Vector4 DefaultTextColor = new(1f, 1f, 1f, 1f);

    // 名牌背景内边距
    private static readonly Vector2 LabelPadding = new(6f, 3f);

    // 名牌背景填充色 (ABGR), 深色半透明
    private const uint LabelBackgroundColor = 0xC8000000;

    // 名牌边框色 (ABGR), 浅色半透明
    private const uint LabelBorderColor = 0x50FFFFFF;

    #endregion
}

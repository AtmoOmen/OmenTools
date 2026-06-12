using System.Numerics;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     区域物体标记的内部条目, 可变以支持就地更新文字与绘制逻辑
/// </summary>
internal sealed class IndicatorEntry
{
    private IndicatorEntry
    (
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Vector3                           fixedPosition,
        Func<IGameObject?>?               objectGetter,
        string?                           text,
        Vector4                           textColor,
        Action<ZoneIndicatorDrawContext>? onDraw,
        uint                              renderRadius,
        bool                              hiddenWhenBlocked
    )
    {
        ID                = id;
        TerritoryType     = territoryType;
        IsPermanent       = isPermanent;
        FixedPosition     = fixedPosition;
        ObjectGetter      = objectGetter;
        Text              = text;
        TextColor         = textColor;
        OnDraw            = onDraw;
        RenderRadius      = renderRadius;
        HiddenWhenBlocked = hiddenWhenBlocked;
    }

    public ulong ID            { get; }
    public uint  TerritoryType { get; }
    public bool  IsPermanent   { get; }

    public Vector3             FixedPosition { get; }
    public Func<IGameObject?>? ObjectGetter  { get; }

    // 以下三项支持运行期就地更新, 引用赋值读写, 不加锁
    public string?                           Text      { get; set; }
    public Vector4                           TextColor { get; set; }
    public Action<ZoneIndicatorDrawContext>? OnDraw    { get; set; }

    public uint RenderRadius      { get; }
    public bool HiddenWhenBlocked { get; }

    public static IndicatorEntry ForPosition
    (
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Vector3                           position,
        string?                           text,
        Vector4?                          textColor,
        Action<ZoneIndicatorDrawContext>? onDraw,
        ZoneIndicatorOptions?             options
    )
    {
        options ??= ZoneIndicatorOptions.Default;
        return new
        (
            id,
            territoryType,
            isPermanent,
            position,
            null,
            text,
            textColor ?? ImGuiZoneObjectIndicator.DefaultTextColor,
            onDraw,
            options.RenderRadius,
            options.HiddenWhenBlocked
        );
    }

    public static IndicatorEntry ForObject
    (
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Func<IGameObject?>                objectGetter,
        string?                           text,
        Vector4?                          textColor,
        Action<ZoneIndicatorDrawContext>? onDraw,
        ZoneIndicatorOptions?             options
    )
    {
        options ??= ZoneIndicatorOptions.Default;
        return new
        (
            id,
            territoryType,
            isPermanent,
            default,
            objectGetter,
            text,
            textColor ?? ImGuiZoneObjectIndicator.DefaultTextColor,
            onDraw,
            options.RenderRadius,
            options.HiddenWhenBlocked
        );
    }

    // 解析当前世界坐标, 物体获取失败时返回 false
    public bool TryResolvePosition(out Vector3 position)
    {
        if (ObjectGetter == null)
        {
            position = FixedPosition;
            return true;
        }

        var gameObject = ObjectGetter();

        if (gameObject == null || !gameObject.IsValid())
        {
            position = default;
            return false;
        }

        position = gameObject.Position;
        return true;
    }
}

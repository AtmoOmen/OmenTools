using System.Numerics;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     区域物体标记的内部条目
///     身份与生命周期字段只读, 内容字段支持运行期就地更新, 引用赋值读写, 不加锁
/// </summary>
internal sealed class ZoneIndicatorEntry : IZoneIndicatorMutable
{
    private uint renderRadius;

    private ZoneIndicatorEntry
    (
        ulong                                 id,
        uint                                  territoryType,
        bool                                  isPermanent,
        Vector3                               fixedPosition,
        Func<List<IGameObject>>?              objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter,
        Func<Vector3, ZoneIndicatorText>?     posTextGetter,
        Action<ZoneIndicatorDrawContext>?     onDraw,
        uint                                  renderRadius,
        bool                                  hiddenWhenBlocked,
        ZoneIndicatorSurrounding?             surrounding
    )
    {
        ID                = id;
        TerritoryType     = territoryType;
        IsPermanent       = isPermanent;
        FixedPosition     = fixedPosition;
        ObjectGetter      = objectGetter;
        ObjTextGetter     = objTextGetter;
        PosTextGetter     = posTextGetter;
        OnDraw            = onDraw;
        RenderRadius      = renderRadius;
        HiddenWhenBlocked = hiddenWhenBlocked;
        Surrounding       = surrounding;
    }

    public ulong ID            { get; }
    public uint  TerritoryType { get; }
    public bool  IsPermanent   { get; }

    public Vector3                  FixedPosition { get; set; }
    public Func<List<IGameObject>>? ObjectGetter  { get; set; }

    public Func<IGameObject, ZoneIndicatorText>? ObjTextGetter { get; set; }
    public Func<Vector3, ZoneIndicatorText>?     PosTextGetter { get; set; }

    public Action<ZoneIndicatorDrawContext>? OnDraw { get; set; }

    public uint RenderRadius
    {
        get => renderRadius;
        set
        {
            renderRadius        = value;
            RenderRadiusSquared = (float)value * value;
        }
    }

    /// <summary>
    ///     渲染半径平方, 随 <see cref="RenderRadius" /> 自动更新, 供距离剔除直接比较
    /// </summary>
    public float RenderRadiusSquared { get; private set; }

    public bool HiddenWhenBlocked { get; set; }

    public ZoneIndicatorSurrounding? Surrounding { get; set; }

    public static ZoneIndicatorEntry ForPosition
    (
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Vector3                           position,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
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
            null,
            posTextGetter,
            onDraw,
            options.RenderRadius,
            options.HiddenWhenBlocked,
            options.Surrounding
        );
    }

    public static ZoneIndicatorEntry ForObject
    (
        ulong                                 id,
        uint                                  territoryType,
        bool                                  isPermanent,
        Func<List<IGameObject>>               objectGetter,
        Func<IGameObject, ZoneIndicatorText>? objTextGetter = null,
        Action<ZoneIndicatorDrawContext>?     onDraw        = null,
        ZoneIndicatorOptions?                 options       = null
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
            objTextGetter,
            null,
            onDraw,
            options.RenderRadius,
            options.HiddenWhenBlocked,
            options.Surrounding
        );
    }

    /// <summary>
    ///     解析所有目标位置及对应物体引用
    ///     固定位置条目返回单一 (FixedPosition, null)
    ///     跟随物体条目返回每个有效物体的 (Position, GameObject)
    /// </summary>
    public IEnumerable<(Vector3 Position, IGameObject? GameObject)> ResolveTargets()
    {
        if (ObjectGetter == null)
        {
            yield return (FixedPosition, null);
            yield break;
        }

        var objects = ObjectGetter();
        if (objects is not { Count: > 0 })
            yield break;

        foreach (var go in objects)
        {
            if (go == null || !go.IsValid())
                continue;

            yield return (go.Position, go);
        }
    }
}

using System.Numerics;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     区域物体标记的内部条目
///     身份与生命周期字段只读, 内容字段支持运行期就地更新, 引用赋值读写, 不加锁
/// </summary>
internal sealed unsafe class ZoneIndicatorEntry : IZoneIndicatorMutable
{
    private ZoneIndicatorEntry
    (
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Func<List<Vector3>>?              positionGetter,
        Func<List<nint>>?                 objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter,
        Func<Vector3, ZoneIndicatorText>? posTextGetter,
        Action<ZoneIndicatorDrawContext>? onDraw,
        uint                              renderRadius,
        bool                              hiddenWhenBlocked,
        ZoneIndicatorSurrounding?         surrounding
    )
    {
        ID                = id;
        TerritoryType     = territoryType;
        IsPermanent       = isPermanent;
        PositionGetter    = positionGetter;
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

    public Func<List<Vector3>>? PositionGetter { get; set; }
    public Func<List<nint>>?    ObjectGetter   { get; set; }

    public Func<nint, ZoneIndicatorText>?    ObjTextGetter { get; set; }
    public Func<Vector3, ZoneIndicatorText>? PosTextGetter { get; set; }

    public Action<ZoneIndicatorDrawContext>? OnDraw { get; set; }

    public uint RenderRadius
    {
        get;
        set
        {
            field               = value;
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
        Func<List<Vector3>>               positionGetter,
        Func<Vector3, ZoneIndicatorText>? posTextGetter = null,
        Action<ZoneIndicatorDrawContext>? onDraw        = null,
        ZoneIndicatorOptions?             options       = null
    )
    {
        ArgumentNullException.ThrowIfNull(positionGetter);
        options ??= ZoneIndicatorOptions.Default;
        return new
        (
            id,
            territoryType,
            isPermanent,
            positionGetter,
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
        ulong                             id,
        uint                              territoryType,
        bool                              isPermanent,
        Func<List<nint>>                  objectGetter,
        Func<nint, ZoneIndicatorText>?    objTextGetter = null,
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
            null,
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
    ///     解析所有目标位置及对应物体指针
    ///     位置条目返回位置获取器提供的每个 (Position, nint.Zero)
    ///     跟随物体条目返回每个有效物体的 (Position, ObjectPtr)
    /// </summary>
    public IEnumerable<(Vector3 Position, nint ObjectPtr)> ResolveTargets()
    {
        if (ObjectGetter == null)
        {
            if (PositionGetter == null)
                yield break;

            var positions = PositionGetter();
            if (positions is not { Count: > 0 })
                yield break;

            foreach (var pos in positions)
                yield return (pos, nint.Zero);

            yield break;
        }

        var ptrs = ObjectGetter();
        if (ptrs is not { Count: > 0 })
            yield break;

        foreach (var ptr in ptrs)
        {
            if (ptr == nint.Zero)
                continue;

            if (!TryGetPosition(ptr, out var pos))
                continue;

            yield return (pos, ptr);
        }
    }

    private static bool TryGetPosition(nint ptr, out Vector3 pos)
    {
        var go = (CSGameObject*)ptr;

        if (go == null)
        {
            pos = default;
            return false;
        }

        pos = go->Position;
        return true;
    }
}

using System.Numerics;

namespace OmenTools.OmenService.ZoneIndicator;

internal abstract class ZoneIndicatorEntryBase : IZoneIndicatorMutable
{
    protected ZoneIndicatorEntryBase
    (
        ulong                id,
        uint                 territoryType,
        bool                 isPermanent,
        ZoneIndicatorOptions options
    )
    {
        ID                = id;
        TerritoryType     = territoryType;
        IsPermanent       = isPermanent;
        RenderRadius      = options.RenderRadius;
        HiddenWhenBlocked = options.HiddenWhenBlocked;
        HideTextLabel     = options.HideTextLabel;
        Surrounding       = options.Surrounding;
    }

    public ulong ID            { get; }
    public uint  TerritoryType { get; }
    public bool  IsPermanent   { get; }

    public uint? RenderRadius
    {
        get;
        set
        {
            field               = value;
            RenderRadiusSquared = value is { } v ? (float)v * v : null;
        }
    }

    /// <summary>渲染半径平方, null 表示不做距离限制</summary>
    public float? RenderRadiusSquared { get; private set; }

    public bool                      HiddenWhenBlocked { get; set; }
    public bool                      HideTextLabel     { get; set; }
    public ZoneIndicatorSurrounding? Surrounding       { get; set; }

    /// <summary>解析所有目标, 每项已含预解析的 <see cref="ZoneIndicatorText" /> 与逐目标 CustomDraw 闭包</summary>
    internal abstract IEnumerable<ResolvedTarget> ResolveTargets();
}

internal sealed class ZoneIndicatorEntry<T> : ZoneIndicatorEntryBase
{
    private readonly Func<List<T>>                        sourceGetter;
    private readonly Func<T, Vector3>                     positionSelector;
    private readonly Func<T, ZoneIndicatorText>?          textGetter;
    private readonly Action<ZoneIndicatorDrawContext<T>>? customDraw;

    public ZoneIndicatorEntry
    (
        ulong                   id,
        uint                    territoryType,
        bool                    isPermanent,
        Func<List<T>>           sourceGetter,
        Func<T, Vector3>        positionSelector,
        ZoneIndicatorOptions<T> options
    ) : base(id, territoryType, isPermanent, options)
    {
        this.sourceGetter     = sourceGetter;
        this.positionSelector = positionSelector;
        textGetter            = options.TextGetter;
        customDraw            = options.CustomDraw;
    }

    internal override IEnumerable<ResolvedTarget> ResolveTargets()
    {
        var sources = sourceGetter();
        if (sources is not { Count: > 0 })
            yield break;

        foreach (var source in sources)
        {
            var pos  = positionSelector(source);
            var text = textGetter?.Invoke(source);

            Action<ZoneIndicatorDrawContext>? perTargetDraw = null;

            if (customDraw is not null)
            {
                var captured = source;
                perTargetDraw = ctx => customDraw(new ZoneIndicatorDrawContext<T>(ctx, captured));
            }

            yield return new ResolvedTarget(pos, text, perTargetDraw);
        }
    }
}

internal readonly struct ResolvedTarget
(
    Vector3                           position,
    ZoneIndicatorText?                text,
    Action<ZoneIndicatorDrawContext>? onDraw
)
{
    public Vector3                           Position { get; } = position;
    public ZoneIndicatorText?                Text     { get; } = text;
    public Action<ZoneIndicatorDrawContext>? OnDraw   { get; } = onDraw;
}

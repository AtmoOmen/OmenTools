using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService;

public sealed class ContextMenuEntryInfo : ContextMenuEntry
{
    private readonly Func<ContextMenuOpenedArgs, ContextMenuItem?>? create;

    private readonly Func<ContextMenuOpenedArgs, IReadOnlyList<ContextMenuItem>?>? createMultiple;

    public ContextMenuEntryInfo
    (
        string                                        identifier,
        Func<ContextMenuOpenedArgs, ContextMenuItem?> createFunc,
        int?                                          priority   = null,
        bool                                          omitPrefix = false,
        ReadOnlySeString?                             prefix     = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentNullException.ThrowIfNull(createFunc);

        Identifier = identifier;
        Priority   = priority;
        OmitPrefix = omitPrefix;
        Prefix     = prefix;
        create     = createFunc;
    }

    public ContextMenuEntryInfo
    (
        string                                                       identifier,
        Func<ContextMenuOpenedArgs, IReadOnlyList<ContextMenuItem>?> createFunc,
        int?                                                         priority   = null,
        bool                                                         omitPrefix = false,
        ReadOnlySeString?                                            prefix     = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentNullException.ThrowIfNull(createFunc);

        Identifier     = identifier;
        Priority       = priority;
        OmitPrefix     = omitPrefix;
        Prefix         = prefix;
        createMultiple = createFunc;
    }

    public override string Identifier { get; }

    public override int? Priority { get; }

    public override bool OmitPrefix { get; }

    public override ReadOnlySeString? Prefix { get; }

    public override ContextMenuItem? Create
    (
        ContextMenuOpenedArgs args
    ) =>
        create?.Invoke(args);

    public override IReadOnlyList<ContextMenuItem>? CreateMultiple
    (
        ContextMenuOpenedArgs args
    ) =>
        createMultiple?.Invoke(args);
}

using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService;

public sealed class ContextMenuEntryInfo : ContextMenuEntry
{
    private readonly Func<ContextMenuOpenedArgs, ContextMenuItem?> create;

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

    public override string Identifier { get; }

    public override int? Priority { get; }

    public override bool OmitPrefix { get; }

    public override ReadOnlySeString? Prefix { get; }

    public override ContextMenuItem? Create
    (
        ContextMenuOpenedArgs args
    ) =>
        create(args);
}

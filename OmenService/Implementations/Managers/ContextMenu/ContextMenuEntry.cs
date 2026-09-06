using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService;

public abstract class ContextMenuEntry
{
    public abstract string Identifier { get; }

    public virtual int? Priority { get; }

    public virtual bool OmitPrefix { get; }

    public virtual ReadOnlySeString? Prefix { get; }

    public abstract ContextMenuItem? Create
    (
        ContextMenuOpenedArgs args
    );
}

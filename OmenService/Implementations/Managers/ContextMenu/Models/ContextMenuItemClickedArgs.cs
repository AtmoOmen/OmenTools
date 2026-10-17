namespace OmenTools.OmenService;

public sealed class ContextMenuItemClickedArgs
{
    internal ContextMenuItemClickedArgs
    (
        ContextMenuOpenedArgs      source,
        Action<ContextMenuSubmenu> inOpenSubmenu
    )
    {
        Source      = source;
        openSubmenu = inOpenSubmenu;
    }

    public ContextMenuOpenedArgs Source { get; }

    public void OpenSubmenu
    (
        ContextMenuSubmenu submenu
    ) =>
        openSubmenu(submenu);

    private readonly Action<ContextMenuSubmenu> openSubmenu;
}

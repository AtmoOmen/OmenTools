using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService;

public sealed class ContextMenuItem
{
    public required ReadOnlySeString Name { get; init; }

    public ReadOnlySeString? Prefix { get; init; }

    public bool IsEnabled { get; init; } = true;

    public bool IsReturn { get; init; }

    public int Priority { get; init; }

    public ContextMenuSubmenu? Submenu { get; init; }

    public Action<ContextMenuItemClickedArgs>? OnClicked { get; init; }

    internal ContextMenuEntry? Entry { get; set; }
}

using Lumina.Text.ReadOnly;

namespace OmenTools.OmenService;

public sealed class ContextMenuSubmenu
{
    public required ReadOnlySeString Title { get; init; }

    public required IReadOnlyList<ContextMenuEntry> Entries { get; init; }
}

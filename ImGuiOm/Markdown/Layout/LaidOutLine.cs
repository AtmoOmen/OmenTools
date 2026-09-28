namespace OmenTools.ImGuiOm.Markdown.Layout;

internal sealed class LaidOutLine
{
    public required IReadOnlyList<LaidOutToken> Tokens { get; init; }

    public required float Width { get; init; }

    public required float Height { get; init; }
}

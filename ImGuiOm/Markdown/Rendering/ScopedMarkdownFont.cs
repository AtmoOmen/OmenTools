namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal sealed class ScopedMarkdownFont : IDisposable
{
    private readonly IDisposable fontPush;

    public bool FauxBold { get; }

    public bool FauxItalic { get; }

    public ImFontPtr Font { get; }

    public float PixelSize { get; }

    public ScopedMarkdownFont
    (
        MarkdownFontRole role,
        float            headingScale,
        MarkdownConfig   config
    )
    {
        // Each size is its own atlas entry, so the glyphs are rendered at that size instead of being
        // scaled up from the body font, which would blur them.
        var handle = config.ResolveFont(role, headingScale);

        // Push() keeps the previous font when this handle is still being built, so the font actually
        // in effect is read back afterwards rather than assumed.
        fontPush  = handle.Push();
        Font      = ImGui.GetFont();
        PixelSize = ImGui.GetFontSize();

        FauxBold   = MarkdownTextDraw.IsBold(role);
        FauxItalic = MarkdownTextDraw.IsItalic(role);
    }

    public void Dispose() => fontPush.Dispose();
}

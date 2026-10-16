using System.Numerics;

namespace OmenTools.ImGuiOm.Markdown.Layout;

public readonly record struct InlineRun
(
    string           Text,
    MarkdownFontRole Role,
    string?          LinkURL,
    bool             IsImage
)
{
    public uint?    IconID        { get; init; }
    public Vector2? ImageSize     { get; init; }
    public bool     ImageInEms    { get; init; }
    public bool     IsLineBreak   { get; init; }
    public bool     Underline     { get; init; }
    public bool     Strikethrough { get; init; }
    public bool     Highlight     { get; init; }
    public bool     Dispel        { get; init; }
    public Vector4? Color         { get; init; }
    public string?  Tooltip       { get; init; }
}

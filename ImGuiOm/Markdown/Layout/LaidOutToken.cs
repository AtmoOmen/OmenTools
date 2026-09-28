namespace OmenTools.ImGuiOm.Markdown.Layout;

internal readonly record struct LaidOutToken
(
    string           Text,
    MarkdownFontRole Role,
    string?          LinkURL,
    bool             IsImage,
    float            X,
    float            Width
);

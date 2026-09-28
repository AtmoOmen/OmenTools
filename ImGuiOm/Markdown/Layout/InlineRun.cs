namespace OmenTools.ImGuiOm.Markdown.Layout;

internal readonly record struct InlineRun
(
    string           Text,
    MarkdownFontRole Role,
    string?          LinkURL,
    bool             IsImage
);

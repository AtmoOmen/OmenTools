using OmenTools.ImGuiOm.Markdown.Parsing;
using MarkdigAst = Markdig.Syntax.MarkdownDocument;

namespace OmenTools.ImGuiOm.Markdown;

public sealed class MarkdownDocument
{
    public string Source { get; }

    internal MarkdigAst Ast { get; }

    public MarkdownDocument
    (
        string markdown
    )
    {
        Source = markdown ?? string.Empty;
        Ast    = MarkdownParser.Parse(Source);
    }

    public void Render
    (
        MarkdownConfig? config = null
    ) => ImGuiMarkdown.Render(this, config);
}

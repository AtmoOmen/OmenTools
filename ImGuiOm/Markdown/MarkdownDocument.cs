using MarkdigAst = Markdig.Syntax.MarkdownDocument;

namespace OmenTools.ImGuiOm.Markdown;

public sealed class MarkdownDocument
{
    private readonly ImGuiMarkdown renderer;

    public string Source { get; }

    internal MarkdigAst Ast { get; }

    public MarkdownDocument
    (
        string         markdown,
        ImGuiMarkdown? renderer = null
    )
    {
        this.renderer = renderer ?? ImGuiMarkdown.Default;
        Source        = markdown ?? string.Empty;
        Ast           = this.renderer.Parser.Parse(Source);
    }

    public void Render
    (
        MarkdownConfig? config = null
    ) => renderer.Draw(this, config);
}

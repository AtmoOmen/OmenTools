using System.Numerics;
using OmenTools.ImGuiOm.Markdown.Parsing;
using OmenTools.ImGuiOm.Markdown.Rendering;

namespace OmenTools.ImGuiOm.Markdown;

public static class ImGuiMarkdown
{
    private static readonly MarkdownConfig DefaultConfig = new();

    public static void Render
    (
        string          markdown,
        MarkdownConfig? config = null
    )
    {
        if (string.IsNullOrEmpty(markdown))
            return;

        var ast = MarkdownParser.GetOrParse(markdown);

        RenderBlocks(ast, config ?? DefaultConfig);
    }

    public static void Render
    (
        MarkdownDocument document,
        MarkdownConfig?  config = null
    ) =>
        RenderBlocks(document.Ast, config ?? DefaultConfig);

    private static void RenderBlocks
    (
        Markdig.Syntax.MarkdownDocument ast,
        MarkdownConfig                  config
    )
    {
        // The document owns its vertical rhythm, so the host window's ItemSpacing is muted for the whole
        // render and every block gap comes from the spacing table. Leaving it in place would add an
        // unrelated fixed distance to each block and flatten the differences between them.
        using var itemSpacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 0.0F));

        BlockRenderer.Render(ast, config);
    }
}

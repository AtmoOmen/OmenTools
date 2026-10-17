using System.Numerics;
using Markdig.Syntax;
using OmenTools.ImGuiOm.Markdown.Parsing;
using OmenTools.ImGuiOm.Markdown.Rendering;

namespace OmenTools.ImGuiOm.Markdown;

public sealed class ImGuiMarkdown
{
    internal static readonly ImGuiMarkdown Default = new();

    internal MarkdownParser Parser { get; }

    private readonly IReadOnlyList<IImGuiMarkdownExtension> extensions;

    public MarkdownConfig Config
    {
        get;
        set => field = value with { Extensions = extensions };
    }

    public ImGuiMarkdown
    (
        IEnumerable<IImGuiMarkdownExtension>? inExtensions = null,
        MarkdownConfig?                       config       = null
    )
    {
        extensions = inExtensions?.ToArray() ?? [];
        Parser     = new(extensions);
        Config     = config ?? new MarkdownConfig();
    }

    public MarkdownDocument Parse
    (
        string markdown
    ) => new(markdown, this);

    public void Draw
    (
        string          markdown,
        MarkdownConfig? config = null
    )
    {
        if (string.IsNullOrEmpty(markdown))
            return;

        RenderBlocks(Parser.GetOrParse(markdown), ResolveConfig(config));
    }

    public void Draw
    (
        MarkdownDocument document,
        MarkdownConfig?  config = null
    ) =>
        RenderBlocks(document.Ast, ResolveConfig(config));

    private MarkdownConfig ResolveConfig
    (
        MarkdownConfig? config
    ) =>
        config is null ?
            Config :
            config with { Extensions = Config.Extensions };

    public static void Render
    (
        string          markdown,
        MarkdownConfig? config = null
    ) => Default.Draw(markdown, config);

    public static void Render
    (
        MarkdownDocument document,
        MarkdownConfig?  config = null
    ) =>
        document.Render(config);

    public static void RenderBlocks
    (
        ContainerBlock ast,
        MarkdownConfig config
    )
    {
        using var id = ImRaii.PushId(ast.GetHashCode());

        // The document owns its vertical rhythm, so the host window's ItemSpacing is muted for the whole
        // render and every block gap comes from the spacing table. Leaving it in place would add an
        // unrelated fixed distance to each block and flatten the differences between them.
        using var itemSpacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 0.0F));

        BlockRenderer.Render(ast, config);
    }
}

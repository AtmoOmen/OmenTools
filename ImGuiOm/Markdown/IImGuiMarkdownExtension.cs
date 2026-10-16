using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OmenTools.ImGuiOm.Markdown.Layout;

namespace OmenTools.ImGuiOm.Markdown;

public interface IImGuiMarkdownExtension
{
    void ConfigurePipeline
    (
        MarkdownPipelineBuilder builder
    );

    string Preprocess
    (
        string markdown
    ) => markdown;

    bool TryRenderBlock
    (
        Block          block,
        MarkdownConfig config
    ) => false;

    bool TryBuildInline
    (
        Inline          inline,
        InlineRun       style,
        List<InlineRun> runs
    ) => false;
}

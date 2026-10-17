using Markdig;
using Markdig.Extensions.EmphasisExtras;
using MarkdigAst = Markdig.Syntax.MarkdownDocument;
using MarkdigParser = Markdig.Markdown;

namespace OmenTools.ImGuiOm.Markdown.Parsing;

internal sealed class MarkdownParser
{
    private readonly MarkdownPipeline                       pipeline;
    private readonly IReadOnlyList<IImGuiMarkdownExtension> extensions;

    private readonly Dictionary<string, MarkdigAst> cache          = [];
    private readonly Queue<string>                  insertionOrder = new();
    private readonly Lock                           gate           = new();

    public MarkdownParser
    (
        IReadOnlyList<IImGuiMarkdownExtension> inExtensions
    )
    {
        extensions = inExtensions;
        var builder = new MarkdownPipelineBuilder()
                      .UsePipeTables()
                      .UseTaskLists()
                      .UseAutoLinks();

        foreach (var extension in extensions)
            extension.ConfigurePipeline(builder);

        pipeline = builder.UseEmphasisExtras(EmphasisExtraOptions.Strikethrough).Build();
    }

    public MarkdigAst Parse
    (
        string markdown
    )
    {
        var source = markdown ?? string.Empty;
        foreach (var extension in extensions)
            source = extension.Preprocess(source);

        return MarkdigParser.Parse(source, pipeline);
    }

    public MarkdigAst GetOrParse
    (
        string markdown
    )
    {
        var key = markdown ?? string.Empty;

        lock (gate)
        {
            if (cache.TryGetValue(key, out var cached))
                return cached;

            var parsed = Parse(key);
            cache[key] = parsed;
            insertionOrder.Enqueue(key);

            while (insertionOrder.Count > MAX_CACHE_ENTRIES)
            {
                var evict = insertionOrder.Dequeue();
                cache.Remove(evict);
            }

            return parsed;
        }
    }

    #region 常量

    private const int MAX_CACHE_ENTRIES = 32;

    #endregion
}

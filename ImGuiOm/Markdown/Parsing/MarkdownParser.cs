using Markdig;
using MarkdigAst = Markdig.Syntax.MarkdownDocument;
using MarkdigParser = Markdig.Markdown;

namespace OmenTools.ImGuiOm.Markdown.Parsing;

internal static class MarkdownParser
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
                                                        .UsePipeTables()
                                                        .UseTaskLists()
                                                        .UseAutoLinks()
                                                        .Build();

    private static readonly Dictionary<string, MarkdigAst> Cache          = [];
    private static readonly Queue<string>                  InsertionOrder = new();
    private static readonly Lock                           Gate           = new();

    public static MarkdigAst Parse
    (
        string markdown
    ) =>
        MarkdigParser.Parse(markdown ?? string.Empty, Pipeline);

    public static MarkdigAst GetOrParse
    (
        string markdown
    )
    {
        var key = markdown ?? string.Empty;

        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var parsed = Parse(key);
            Cache[key] = parsed;
            InsertionOrder.Enqueue(key);

            while (InsertionOrder.Count > MAX_CACHE_ENTRIES)
            {
                var evict = InsertionOrder.Dequeue();
                Cache.Remove(evict);
            }

            return parsed;
        }
    }

    #region 常量

    private const int MAX_CACHE_ENTRIES = 32;

    #endregion
}

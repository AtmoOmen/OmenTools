using Markdig.Syntax.Inlines;
using OmenTools.ImGuiOm.Markdown.Layout;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class InlineBuilder
{
    public static IReadOnlyList<InlineRun> Build
    (
        ContainerInline? container
    )
    {
        List<InlineRun> runs = [];
        if (container is not null)
            Walk(container, runs, false, false, null);

        return runs;
    }

    private static void Walk
    (
        ContainerInline container,
        List<InlineRun> runs,
        bool            bold,
        bool            italic,
        string?         linkURL
    )
    {
        var child = container.FirstChild;

        while (child is not null)
        {
            Append(child, runs, bold, italic, linkURL);
            child = child.NextSibling;
        }
    }

    private static void Append
    (
        Inline          inline,
        List<InlineRun> runs,
        bool            bold,
        bool            italic,
        string?         linkURL
    )
    {
        switch (inline)
        {
            case LiteralInline literal:
                runs.Add(new InlineRun(literal.Content.ToString(), MarkdownTextDraw.EmphasisRole(bold, italic), linkURL, false));
                break;

            case CodeInline code:
                runs.Add(new InlineRun(code.Content, MarkdownFontRole.Code, linkURL, false));
                break;

            case EmphasisInline emphasis:
                var nowBold   = bold   || emphasis.DelimiterCount >= 2;
                var nowItalic = italic || emphasis.DelimiterCount == 1;
                Walk(emphasis, runs, nowBold, nowItalic, linkURL);
                break;

            case LinkInline { IsImage: true } link:
                runs.Add(new InlineRun(link.Url ?? string.Empty, MarkdownFontRole.Body, linkURL, true));
                break;

            case LinkInline link:
                Walk(link, runs, bold, italic, link.Url ?? linkURL);
                break;

            case AutolinkInline autolink:
                runs.Add(new InlineRun(autolink.Url, MarkdownTextDraw.EmphasisRole(bold, italic), autolink.Url, false));
                break;

            case LineBreakInline:
                runs.Add(new InlineRun(" ", MarkdownFontRole.Body, linkURL, false));
                break;

            case HtmlInline html:
                runs.Add(new InlineRun(html.Tag, MarkdownTextDraw.EmphasisRole(bold, italic), linkURL, false));
                break;

            case Markdig.Extensions.TaskLists.TaskList:
                // The checkbox marker is rendered by ListMarker in BlockRenderer; drop the inline itself.
                break;

            case ContainerInline nested:
                Walk(nested, runs, bold, italic, linkURL);
                break;

            default:
                // Unknown inline: emit its text form so nothing is silently dropped.
                runs.Add(new InlineRun(inline.ToString() ?? string.Empty, MarkdownTextDraw.EmphasisRole(bold, italic), linkURL, false));
                break;
        }
    }
}

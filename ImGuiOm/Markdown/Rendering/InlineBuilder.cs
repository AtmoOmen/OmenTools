using Markdig.Syntax.Inlines;
using OmenTools.ImGuiOm.Markdown.Layout;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class InlineBuilder
{
    public static IReadOnlyList<InlineRun> Build
    (
        ContainerInline? container,
        MarkdownConfig   config
    )
    {
        List<InlineRun> runs = [];
        if (container is not null)
            Walk(container, runs, new InlineRun(string.Empty, MarkdownFontRole.Body, null, false), config);

        return runs;
    }

    private static void Walk
    (
        ContainerInline container,
        List<InlineRun> runs,
        InlineRun       style,
        MarkdownConfig  config
    )
    {
        var child = container.FirstChild;

        while (child is not null)
        {
            Append(child, runs, style, config);
            child = child.NextSibling;
        }
    }

    private static void Append
    (
        Inline          inline,
        List<InlineRun> runs,
        InlineRun       style,
        MarkdownConfig  config
    )
    {
        foreach (var extension in config.Extensions)
        {
            if (extension.TryBuildInline(inline, style, runs))
                return;
        }

        switch (inline)
        {
            case LiteralInline literal:
                runs.Add(style with { Text = literal.Content.ToString() });
                break;

            case CodeInline code:
                runs.Add(style with { Text = code.Content, Role = MarkdownFontRole.Code });
                break;

            case EmphasisInline emphasis:
                var next = style;

                switch (emphasis.DelimiterChar)
                {
                    case '~':
                        next = next with { Strikethrough = true };
                        break;
                    case '=':
                        next = next with { Highlight = true };
                        break;
                    case '+':
                        next = next with { Underline = true };
                        break;
                    default:
                        var bold   = MarkdownTextDraw.IsBold(style.Role)   || emphasis.DelimiterCount >= 2;
                        var italic = MarkdownTextDraw.IsItalic(style.Role) || emphasis.DelimiterCount == 1;
                        next = next with { Role = MarkdownTextDraw.EmphasisRole(bold, italic) };
                        break;
                }

                Walk(emphasis, runs, next, config);
                break;

            case LinkInline { IsImage: true } link:
                runs.Add(style with { Text = link.Url ?? string.Empty, IsImage = true, Tooltip = link.Title });
                break;

            case LinkInline link:
                Walk(link, runs, style with { LinkURL = link.Url ?? style.LinkURL }, config);
                break;

            case AutolinkInline autolink:
                runs.Add(style with { Text = autolink.Url, LinkURL = autolink.Url });
                break;

            case LineBreakInline lineBreak:
                if (!lineBreak.IsHard       &&
                    config.JoinCJKSoftLines &&
                    inline is
                    {
                        PreviousSibling: LiteralInline { Content.Length: > 0 } before,
                        NextSibling: LiteralInline { Content.Length: > 0 } after
                    }                                              &&
                    IsCJK(before.Content.Text[before.Content.End]) &&
                    IsCJK(after.Content.CurrentChar))
                    break;

                runs.Add(style with { Text = " ", IsLineBreak = lineBreak.IsHard || config.BreakSoftLines });
                break;

            case HtmlInline html:
                runs.Add(style with { Text = html.Tag });
                break;

            case Markdig.Extensions.TaskLists.TaskList:
                break;

            case ContainerInline nested:
                Walk(nested, runs, style, config);
                break;

            default:
                runs.Add(style with { Text = inline.ToString() ?? string.Empty });
                break;
        }
    }

    private static bool IsCJK
    (
        char character
    ) =>
        character is >= '\u2E80' and <= '\u9FFF' or >= '\uF900' and <= '\uFAFF' or >= '\uFF00' and <= '\uFFEF';
}

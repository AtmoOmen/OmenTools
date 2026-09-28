using System.Numerics;
using System.Text;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using MarkdownCodeBlock = Markdig.Syntax.CodeBlock;
using MarkdownTable = Markdig.Extensions.Tables.Table;
using MarkdownTableCell = Markdig.Extensions.Tables.TableCell;
using MarkdownTableRow = Markdig.Extensions.Tables.TableRow;
using OmenTools.ImGuiOm.Markdown.Parsing;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class BlockRenderer
{
    public static void Render
    (
        ContainerBlock blocks,
        MarkdownConfig config
    )
    {
        MarkdownBlockKind? previous = null;

        foreach (var block in blocks)
        {
            var kind = KindOf(block);

            if (previous.HasValue)
                ImGui.Dummy(new Vector2(0.0F, MarkdownConfig.ResolveGap(previous.Value, kind)));

            RenderBlock(block, config);

            previous = kind;
        }
    }

    private static MarkdownBlockKind KindOf
    (
        Block block
    ) => block switch
    {
        HeadingBlock heading => heading.Level switch
        {
            <= 1 => MarkdownBlockKind.Heading1,
            2    => MarkdownBlockKind.Heading2,
            3    => MarkdownBlockKind.Heading3,
            4    => MarkdownBlockKind.Heading4,
            5    => MarkdownBlockKind.Heading5,
            _    => MarkdownBlockKind.Heading6
        },
        ParagraphBlock     => MarkdownBlockKind.Paragraph,
        ListBlock          => MarkdownBlockKind.List,
        ListItemBlock      => MarkdownBlockKind.ListItem,
        QuoteBlock         => MarkdownBlockKind.Quote,
        MarkdownCodeBlock  => MarkdownBlockKind.Code,
        ThematicBreakBlock => MarkdownBlockKind.ThematicBreak,
        MarkdownTable      => MarkdownBlockKind.Table,
        _                  => MarkdownBlockKind.Paragraph
    };

    private static void RenderBlock
    (
        Block          block,
        MarkdownConfig config
    )
    {
        switch (block)
        {
            case HeadingBlock heading:
                RenderHeading(heading, config);
                break;

            case ParagraphBlock paragraph:
                InlineRenderer.Render(paragraph.Inline, config);
                break;

            case ListBlock list:
                RenderList(list, config);
                break;

            case QuoteBlock quote:
                RenderQuote(quote, config);
                break;

            case MarkdownCodeBlock code:
                RenderCodeBlock(code, config);
                break;

            case ThematicBreakBlock:
                RenderThematicBreak();
                break;

            case MarkdownTable table:
                RenderTable(table, config);
                break;

            case HtmlBlock html:
                RenderHTMLAsText(html, config);
                break;

            case ContainerBlock container:
                Render(container, config);
                break;
        }
    }

    private static void RenderHeading
    (
        HeadingBlock   heading,
        MarkdownConfig config
    )
    {
        var scale = config.ResolveHeadingScale(heading.Level);

        InlineRenderer.Render(heading.Inline, config, scale);
    }

    private static void RenderList
    (
        ListBlock      list,
        MarkdownConfig config
    )
    {
        var start = 1;
        if (list.IsOrdered && int.TryParse(list.OrderedStart, out var parsed))
            start = parsed;

        // Content sits in a gutter at least as wide as the widest marker in this list, so the marker
        // never overlaps the text (e.g. "[x]" is wider than the default indent) and every item aligns
        // to the same content column. ImGui.Indent resets the cursor X, so the gutter, not SameLine,
        // determines where content begins.
        var gutter       = config.ListIndentPixels;
        var measureIndex = 0;

        foreach (var probe in list)
        {
            if (probe is ListItemBlock probeItem)
            {
                var probeMarker = ListMarker.For(list.IsOrdered, measureIndex, start, TryGetTaskState(probeItem));
                gutter = MathF.Max(gutter, ImGui.CalcTextSize(probeMarker).X + ImGui.GetStyle().ItemSpacing.X);
                measureIndex++;
            }
        }

        var index = 0;

        foreach (var item in list)
        {
            if (item is ListItemBlock listItem)
            {
                if (index > 0)
                    ImGui.Dummy(new Vector2(0.0F, MarkdownConfig.ResolveGap(MarkdownBlockKind.ListItem, MarkdownBlockKind.ListItem)));

                var taskChecked = TryGetTaskState(listItem);
                var marker      = ListMarker.For(list.IsOrdered, index, start, taskChecked);

                ImGui.TextUnformatted(marker);
                ImGui.SameLine();
                ImGui.Indent(gutter);
                Render(listItem, config);
                ImGui.Unindent(gutter);
                index++;
            }
        }
    }

    private static bool? TryGetTaskState
    (
        ListItemBlock listItem
    )
    {
        foreach (var child in listItem)
        {
            if (child is ParagraphBlock { Inline: not null } paragraph)
            {
                var inline = paragraph.Inline.FirstChild;

                while (inline is not null)
                {
                    if (inline is TaskList task)
                        return task.Checked;

                    inline = inline.NextSibling;
                }
            }
        }

        return null;
    }

    private static void RenderQuote
    (
        QuoteBlock     quote,
        MarkdownConfig config
    )
    {
        var start = ImGui.GetCursorScreenPos();
        ImGui.Indent(config.ListIndentPixels);
        Render(quote, config);
        ImGui.Unindent(config.ListIndentPixels);

        var end      = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        var barX     = start.X + (config.ListIndentPixels * 0.35F);
        drawList.AddLine(start with { X = barX }, end with { X = barX }, MarkdownColors.BlockquoteBar(), 2.0F);
    }

    private static void RenderCodeBlock
    (
        MarkdownCodeBlock code,
        MarkdownConfig    config
    )
    {
        var text = ExtractCodeText(code);

        if (config.CodeBlockRenderer is not null)
        {
            // The hook owns the block completely — drawing and reserving space — so a highlighter can
            // paint its own background, gutter and colors instead of the shaded monospace panel below.
            config.CodeBlockRenderer((code as FencedCodeBlock)?.Info, text);
            return;
        }

        const float PAD        = 4.0F;
        var         start      = ImGui.GetCursorScreenPos();
        var         availWidth = ImGui.GetContentRegionAvail().X;

        using (new ScopedMarkdownFont(MarkdownFontRole.Code, 1.0F, config))
        {
            var textSize    = ImGui.CalcTextSize(text);
            var blockHeight = textSize.Y + (PAD * 2.0F);
            var drawList    = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(start, start + new Vector2(availWidth, blockHeight), MarkdownColors.InlineCodeBackground(), 3.0F);

            // Position the text at an explicit padded offset. Using Dummy + TextUnformatted as separate
            // items previously let ImGui insert ItemSpacing.Y between them, pushing the text below the
            // shaded background. Drawing at start + pad and then reserving the full footprint avoids that.
            ImGui.SetCursorScreenPos(new Vector2(start.X + PAD, start.Y + PAD));
            ImGui.TextUnformatted(text);

            ImGui.SetCursorScreenPos(start);
            ImGui.Dummy(new Vector2(availWidth, blockHeight));
        }
    }

    // LeafBlock (not CodeBlock specifically) because HtmlBlock shares the same Lines field and
    // is rendered as plain text via this same extraction path.
    private static string ExtractCodeText
    (
        LeafBlock block
    )
    {
        StringBuilder builder = new();
        var           lines   = block.Lines;
        for (var i = 0; i < lines.Count; i++)
            builder.AppendLine(lines.Lines[i].ToString());

        return builder.ToString().TrimEnd('\n', '\r');
    }

    private static void RenderThematicBreak()
    {
        var start = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var y     = start.Y + 4.0F;
        ImGui.GetWindowDrawList().AddLine(start with { Y = y }, start with { X = start.X + width, Y = y }, MarkdownColors.Separator(), 1.0F);
        ImGui.Dummy(new Vector2(width, 9.0F));
    }

    private static void RenderTable
    (
        MarkdownTable  table,
        MarkdownConfig config
    )
    {
        var columnCount = table.ColumnDefinitions.Count;

        if (columnCount == 0)
        {
            foreach (var rowBlock in table)
            {
                if (rowBlock is MarkdownTableRow row)
                    columnCount = Math.Max(columnCount, row.Count);
            }
        }

        if (columnCount == 0)
            return;

        const ImGuiTableFlags FLAGS = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp;

        // Each table needs a unique ImGui id: the literal "##mdtable" would otherwise be shared by
        // every table at the same ID-stack level, colliding their column widths and resize state.
        // table.Span.Start is a stable, per-table-unique offset into the parsed source.
        ImGui.PushID(table.Span.Start);

        try
        {
            if (ImGui.BeginTable("##mdtable", columnCount, FLAGS))
            {
                foreach (var rowBlock in table)
                {
                    if (rowBlock is MarkdownTableRow row)
                    {
                        ImGui.TableNextRow();
                        using var headerFont = row.IsHeader ?
                                                   new ScopedMarkdownFont(MarkdownFontRole.Bold, 1.0F, config) :
                                                   null;

                        foreach (var cellBlock in row)
                        {
                            if (cellBlock is MarkdownTableCell cell)
                            {
                                ImGui.TableNextColumn();

                                foreach (var content in cell)
                                {
                                    if (content is ParagraphBlock paragraph)
                                        InlineRenderer.Render(paragraph.Inline, config);
                                }
                            }
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
        finally
        {
            ImGui.PopID();
        }
    }

    private static void RenderHTMLAsText
    (
        HtmlBlock      html,
        MarkdownConfig config
    )
    {
        var text = ExtractCodeText(html);

        using (new ScopedMarkdownFont(MarkdownFontRole.Code, 1.0F, config))
        {
            ImGui.TextUnformatted(text);
        }
    }
}

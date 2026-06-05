using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using OmenTools.OmenService;

namespace OmenTools.ImGuiOm.Widgets.MarkdownRenderer;

public static class ImGuiMarkdownRenderer
{
    public static void Render(string source)
    {
        foreach (var block in ParseBlocks(source))
            RenderBlock(block);
    }

    
    private static readonly SearchValues<char> MarkerChars = SearchValues.Create('*', '_', '[', '!');

    private static IEnumerable<Block> ParseBlocks(string source)
    {
        var start = 0;

        while (start < source.Length)
        {
            var end = source.IndexOf('\n', start);

            if (end < 0)
                end = source.Length;

            var line = source.AsSpan(start, end - start).TrimEnd('\r');
            start = end + 1;

            Block? block = null;

            if (line.IsEmpty)
                block = new Paragraph([]);
            else if (line is "***" or "___" or "---")
                block = new ThematicBreak();
            else if (StartsHeading(line, out var headingLevel, out var headingText))
                block = new Heading(headingLevel, headingText.ToString());
            else if (StartsListItem(line, out var listDepth, out var listItemText))
                block = new ListItem(listDepth, ParseInlines(listItemText));
            else
                block = new Paragraph(ParseInlines(line));

            if (block != null)
                yield return block;
        }
    }

    private static bool StartsHeading(ReadOnlySpan<char> line, out int level, out ReadOnlySpan<char> headingText)
    {
        level       = 0;
        headingText = default;

        var sharpCount = 0;

        while (sharpCount < line.Length && line[sharpCount] == '#')
            sharpCount++;

        if (sharpCount == 0 || sharpCount > 6 || sharpCount >= line.Length || line[sharpCount] != ' ')
            return false;

        level       = sharpCount;
        headingText = line[(sharpCount + 1)..].TrimStart();

        return headingText.Length > 0;
    }

    private static bool StartsListItem(ReadOnlySpan<char> line, out int depth, out ReadOnlySpan<char> text)
    {
        depth = 0;
        text  = default;

        var spaceCount = 0;

        while (spaceCount < line.Length && line[spaceCount] == ' ')
            spaceCount++;

        if (spaceCount < 2)
            return false;

        if (spaceCount >= line.Length || line[spaceCount] != '*' || spaceCount + 1 >= line.Length || line[spaceCount + 1] != ' ')
            return false;

        depth = (spaceCount / 2) - 1;
        text  = line[(spaceCount + 2)..];

        return true;
    }

    private static IReadOnlyList<Inline> ParseInlines(ReadOnlySpan<char> text)
    {
        var items    = new List<Inline>();
        var position = 0;

        while (position < text.Length)
        {
            if (TryReadSpecial(text, position, out var inline, out var consumed))
            {
                if (inline != null)
                    items.Add(inline);

                position += consumed;

                continue;
            }

            var nextMarker = text[position..].IndexOfAny(MarkerChars);

            if (nextMarker < 0)
                nextMarker = text.Length - position;

            if (nextMarker > 0)
            {
                items.Add(new TextRun(text.Slice(position, nextMarker).ToString()));
                position += nextMarker;
            }
            else
            {
                items.Add(new TextRun(text[position..].ToString()));

                break;
            }
        }

        return items;
    }

    private static bool TryReadSpecial(ReadOnlySpan<char> text, int position, out Inline? inline, out int consumed)
    {
        inline   = null;
        consumed = 0;

        if (TryReadImage(text, position, out var imageAlt, out var imageSource, out var imageLength))
        {
            inline   = new Image(imageAlt.ToString(), imageSource.ToString());
            consumed = imageLength;

            return true;
        }

        if (TryReadLink(text, position, out var linkText, out var linkTarget, out var linkLength))
        {
            inline   = new Hyperlink(linkText.ToString(), linkTarget.ToString());
            consumed = linkLength;

            return true;
        }

        if (TryReadStrong(text, position, out var strongText, out var strongLength))
        {
            inline   = new Strong(strongText.ToString());
            consumed = strongLength;

            return true;
        }

        if (TryReadEmphasis(text, position, out var emphasisText, out var emphasisLength))
        {
            inline   = new Emphasis(emphasisText.ToString());
            consumed = emphasisLength;

            return true;
        }

        return false;
    }

    private static bool TryReadImage(ReadOnlySpan<char> text, int position, out ReadOnlySpan<char> alt, out ReadOnlySpan<char> source, out int length)
    {
        alt    = default;
        source = default;
        length = 0;

        if (position + 2 >= text.Length || text[position] != '!' || text[position + 1] != '[')
            return false;

        var closingBracket = text[(position + 2)..].IndexOf(']');

        if (closingBracket < 0)
            return false;

        var parenthesis = text[(position + 2 + closingBracket + 1)..];

        if (parenthesis.Length < 2 || parenthesis[0] != '(')
            return false;

        var closingParen = parenthesis.IndexOf(')');

        if (closingParen < 0)
            return false;

        alt    = text.Slice(position + 2, closingBracket);
        source = parenthesis.Slice(1, closingParen);
        length = 2 + closingBracket + 1 + 1 + closingParen + 1;

        return true;
    }

    private static bool TryReadLink(ReadOnlySpan<char> text, int position, out ReadOnlySpan<char> display, out ReadOnlySpan<char> target, out int length)
    {
        display = default;
        target  = default;
        length  = 0;

        if (position >= text.Length || text[position] != '[')
            return false;

        var closingBracket = text[(position + 1)..].IndexOf(']');

        if (closingBracket < 0)
            return false;

        var parenthesis = text[(position + 1 + closingBracket + 1)..];

        if (parenthesis.Length < 2 || parenthesis[0] != '(')
            return false;

        var closingParen = parenthesis.IndexOf(')');

        if (closingParen < 0)
            return false;

        display = text.Slice(position + 1, closingBracket);
        target  = parenthesis.Slice(1, closingParen);
        length  = 1 + closingBracket + 1 + 1 + closingParen + 1;

        return true;
    }

    private static bool TryReadStrong(ReadOnlySpan<char> text, int position, out ReadOnlySpan<char> content, out int length)
    {
        content = default;
        length  = 0;

        if (position + 3 >= text.Length || text[position] != '*' || text[position + 1] != '*')
            return false;

        var closing = text[(position + 2)..].IndexOf("**");

        if (closing < 0)
            return false;

        content = text.Slice(position + 2, closing);
        length  = 2 + closing + 2;

        return true;
    }

    private static bool TryReadEmphasis(ReadOnlySpan<char> text, int position, out ReadOnlySpan<char> content, out int length)
    {
        content = default;
        length  = 0;

        if (position >= text.Length || (text[position] != '*' && text[position] != '_'))
            return false;

        var symbol  = text[position];
        var closing = text[(position + 1)..].IndexOf(symbol);

        if (closing < 0)
            return false;

        content = text.Slice(position + 1, closing);
        length  = 1 + closing + 1;

        return true;
    }

    private static void RenderBlock(Block block)
    {
        switch (block)
        {
            case Heading heading:
                RenderHeading(heading);
                break;

            case Paragraph paragraph:
                RenderParagraph(paragraph);
                break;

            case ListItem listItem:
                RenderListItem(listItem);
                break;

            case ThematicBreak:
                ImGui.Separator();
                break;
        }
    }

    private static void RenderHeading(Heading heading)
    {
        var fontManager = FontManager.Instance();
        var font = heading.Level switch
        {
            1 => fontManager.UIFont120,
            2 => fontManager.UIFont,
            _ => fontManager.UIFont80
        };

        ImGui.NewLine();
        font?.Push();
        ImGui.TextUnformatted(heading.Text);

        if (font != null)
            ImGui.PopFont();

        if (heading.Level <= 2)
        {
            var cursor = ImGui.GetCursorPos();
            ImGui.Separator();
            ImGui.SetCursorPos(cursor);
        }

        ImGui.NewLine();
    }

    private static void RenderParagraph(Paragraph paragraph)
    {
        if (paragraph.Inlines.Count == 0)
        {
            ImGui.NewLine();

            return;
        }

        var wrapWidth = ImGui.GetContentRegionAvail().X;

        if (wrapWidth > 0)
            ImGui.PushTextWrapPos(ImGui.GetCursorScreenPos().X + wrapWidth);

        foreach (var inline in paragraph.Inlines)
            RenderInline(inline);

        if (wrapWidth > 0)
            ImGui.PopTextWrapPos();

        ImGui.NewLine();
    }

    private static void RenderListItem(ListItem listItem)
    {
        for (var depthIndex = 0; depthIndex < listItem.Depth; depthIndex++)
            ImGui.Indent();

        ImGui.Bullet();
        ImGui.SameLine();

        var wrapWidth = ImGui.GetContentRegionAvail().X;

        if (wrapWidth > 0)
            ImGui.PushTextWrapPos(ImGui.GetCursorScreenPos().X + wrapWidth);

        foreach (var inline in listItem.Inlines)
            RenderInline(inline);

        if (wrapWidth > 0)
            ImGui.PopTextWrapPos();

        for (var depthIndex = 0; depthIndex < listItem.Depth; depthIndex++)
            ImGui.Unindent();
    }

    private static void RenderInline(Inline inline)
    {
        switch (inline)
        {
            case TextRun textRun:
                ImGui.TextUnformatted(textRun.Content);
                return;

            case Emphasis emphasis:
                ImGui.PushStyleColor
                (
                    ImGuiCol.Text,
                    ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]
                );
                ImGui.TextUnformatted(emphasis.Content);
                ImGui.PopStyleColor();
                return;

            case Strong strong:
                using (FontManager.Instance().UIFont80?.Push())
                    ImGui.TextUnformatted(strong.Content);

                return;

            case Hyperlink hyperlink:
                ImGui.PushStyleColor
                (
                    ImGuiCol.Text,
                    ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered]
                );
                ImGui.TextUnformatted(hyperlink.Display);
                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

                    var min = ImGui.GetItemRectMin();
                    var max = ImGui.GetItemRectMax();

                    min.Y = max.Y;

                    ImGui.GetWindowDrawList().AddLine
                    (
                        min,
                        max,
                        ImGui.GetColorU32(ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered]),
                        1.0f
                    );

                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        OpenURL(hyperlink.Target);
                }

                return;

            case Image image:
                RenderImage(image);
                return;
        }
    }

    private static void RenderImage(Image image)
    {
        if (string.IsNullOrEmpty(image.Source))
        {
            ImGui.Text($"(Image: {image.Alt})");

            return;
        }

        var texture = ImageHelper.Instance().GetImage(image.Source);

        if (texture == null)
        {
            ImGui.Text($"(Image: {image.Source})");

            return;
        }

        var imageSize     = new Vector2(texture.Width, texture.Height);
        var availableArea = ImGui.GetContentRegionAvail();

        if (imageSize.X > availableArea.X && availableArea.X > 0)
        {
            var aspectRatio = imageSize.Y / imageSize.X;
            imageSize.X = availableArea.X;
            imageSize.Y = availableArea.X * aspectRatio;
        }

        ImGui.Image(texture.Handle, imageSize);

        if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            OpenURL(image.Source);
    }

    private static void OpenURL(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // ignored
        }
    }

    private abstract record Block;

    private sealed record Heading
    (
        int    Level,
        string Text
    ) : Block;

    private sealed record Paragraph
    (
        IReadOnlyList<Inline> Inlines
    ) : Block;

    private sealed record ListItem
    (
        int                   Depth,
        IReadOnlyList<Inline> Inlines
    ) : Block;

    private sealed record ThematicBreak : Block;

    private abstract record Inline;

    private sealed record TextRun
    (
        string Content
    ) : Inline;

    private sealed record Emphasis
    (
        string Content
    ) : Inline;

    private sealed record Strong
    (
        string Content
    ) : Inline;

    private sealed record Hyperlink
    (
        string Display,
        string Target
    ) : Inline;

    private sealed record Image
    (
        string Alt,
        string Source
    ) : Inline;
}

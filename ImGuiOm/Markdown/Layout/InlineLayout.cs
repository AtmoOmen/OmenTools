using System.Numerics;

namespace OmenTools.ImGuiOm.Markdown.Layout;

internal static class InlineLayout
{
    public static IReadOnlyList<LaidOutLine> Wrap
    (
        IReadOnlyList<InlineRun>                      runs,
        float                                         maxWidth,
        Func<string, MarkdownFontRole, bool, Vector2> measure,
        Func<MarkdownFontRole, float>                 runOverhang
    )
    {
        var spaceWidth = measure(" ", MarkdownFontRole.Body, false).X;

        List<LaidOutLine>  lines      = [];
        List<LaidOutToken> current    = [];
        var                cursorX    = 0.0F;
        var                lineHeight = 0.0F;

        // Carried across runs: Markdig splits a literal like "优化 [生产采集]" into separate runs at
        // the bracket, so a trailing space in one run only survives if the pending flag is not reset.
        var pendingSpace = false;

        foreach (var run in runs)
        {
            if (run.IsImage)
            {
                Place(run.Text, run, measure, maxWidth, spaceWidth, lines, current, ref cursorX, ref lineHeight, pendingSpace);
                pendingSpace = false;
                continue;
            }

            foreach (var (text, isSpace) in SplitTokens(run.Text))
            {
                if (isSpace)
                {
                    pendingSpace = true;
                    continue;
                }

                Place(text, run, measure, maxWidth, spaceWidth, lines, current, ref cursorX, ref lineHeight, pendingSpace);
                pendingSpace = false;
            }

            // The italic lean and bold stroke both reach past the last glyph's advance, but only the run
            // as a whole spends that once. Charging it per token would space out CJK, which is laid out
            // one character per token.
            if (current.Count > 0)
                cursorX += runOverhang(run.Role);
        }

        FlushLine(lines, current, ref cursorX, ref lineHeight);
        return lines;
    }

    // A space is emitted only where the source actually had one. Treating every token boundary as a
    // space would insert gaps between the per-character tokens that CJK text is split into.
    private static void Place
    (
        string                                        text,
        InlineRun                                     run,
        Func<string, MarkdownFontRole, bool, Vector2> measure,
        float                                         maxWidth,
        float                                         spaceWidth,
        List<LaidOutLine>                             lines,
        List<LaidOutToken>                            current,
        ref float                                     cursorX,
        ref float                                     lineHeight,
        bool                                          wantsLeadingSpace
    )
    {
        var tokenSize  = measure(text, run.Role, run.IsImage);
        var tokenWidth = tokenSize.X;
        var advance = wantsLeadingSpace && current.Count > 0 ?
                          spaceWidth :
                          0.0F;

        if (current.Count > 0 && cursorX + advance + tokenWidth > maxWidth)
        {
            FlushLine(lines, current, ref cursorX, ref lineHeight);
            advance = 0.0F;
        }

        var x = cursorX + advance;
        current.Add(new LaidOutToken(text, run.Role, run.LinkURL, run.IsImage, x, tokenWidth));
        cursorX    = x + tokenWidth;
        lineHeight = Math.Max(lineHeight, tokenSize.Y);
    }

    private static void FlushLine
    (
        List<LaidOutLine>  lines,
        List<LaidOutToken> current,
        ref float          cursorX,
        ref float          lineHeight
    )
    {
        if (current.Count > 0)
        {
            LaidOutToken[] tokens = [.. current];
            lines.Add(new LaidOutLine { Tokens = tokens, Width = cursorX, Height = lineHeight });
        }

        current.Clear();
        cursorX    = 0.0F;
        lineHeight = 0.0F;
    }

    // Latin words are kept whole so they are not broken mid-word, whitespace is reported as its own
    // marker so the layout knows a real space preceded the next token, and CJK glyphs are emitted one
    // character per token because CJK text has no spaces and must be breakable between any two
    // characters.
    private static IEnumerable<(string Text, bool IsSpace)> SplitTokens
    (
        string text
    )
    {
        var wordStart = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsWhiteSpace(c))
            {
                if (i > wordStart)
                    yield return (text[wordStart..i], false);

                yield return (string.Empty, true);
                wordStart = i + 1;
            }
            else if (IsCJK(c))
            {
                if (i > wordStart)
                    yield return (text[wordStart..i], false);

                yield return (c.ToString(), false);
                wordStart = i + 1;
            }
        }

        if (wordStart < text.Length)
            yield return (text[wordStart..], false);
    }

    private static bool IsCJK
    (
        char c
    ) =>
        c is >= '\u1100' and <= '\u11FF' // Hangul Jamo
        ||
        c is >= '\u2E80' and <= '\u303F' // CJK Radicals, Kangxi, CJK Symbols and Punctuation
        ||
        c is >= '\u3040' and <= '\u30FF' // Hiragana, Katakana
        ||
        c is >= '\u3130' and <= '\u318F' // Hangul Compatibility Jamo
        ||
        c is >= '\u3400' and <= '\u4DBF' // CJK Unified Ideographs Extension A
        ||
        c is >= '\u4E00' and <= '\u9FFF' // CJK Unified Ideographs
        ||
        c is >= '\uAC00' and <= '\uD7AF' // Hangul Syllables
        ||
        c is >= '\uF900' and <= '\uFAFF' // CJK Compatibility Ideographs
        ||
        c is >= '\uFF00' and <= '\uFFEF'; // Halfwidth and Fullwidth Forms
}

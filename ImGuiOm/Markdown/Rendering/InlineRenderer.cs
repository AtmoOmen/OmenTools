using System.Numerics;
using Markdig.Syntax.Inlines;
using OmenTools.ImGuiOm.Markdown.Layout;
using OmenTools.ImGuiOm.Markdown.Parsing;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class InlineRenderer
{
    public static void Render
    (
        ContainerInline? inline,
        MarkdownConfig   config,
        float            headingScale = 1.0F
    )
    {
        var runs = InlineBuilder.Build(inline, config);
        if (runs.Count == 0)
            return;

        var wrapWidth = config.WrapWidth ?? ImGui.GetContentRegionAvail().X;
        if (wrapWidth <= 0.0F)
            wrapWidth = 1.0F;

        var lineSpacing = ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight();

        var lines = InlineLayout.Wrap
        (
            runs,
            wrapWidth,
            (text, run) => Measure(text, run, config, headingScale, wrapWidth),
            role => Overhang(role, headingScale, config)
        );

        var origin   = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        var linkU32  = ImGui.GetColorU32(MarkdownColors.Link(config));
        var textU32  = MarkdownColors.TextU32();

        var y         = 0.0F;
        var linkIndex = 0;

        foreach (var line in lines)
        {
            foreach (var token in line.Tokens)
            {
                Vector2 pos = new(origin.X + token.X, origin.Y + y);
                var color = token.LinkURL is null ?
                                textU32 :
                                linkU32;
                if (token.Style.Color.HasValue)
                    color = ImGui.GetColorU32(token.Style.Color.Value);

                DrawToken
                (
                    token,
                    pos,
                    config,
                    drawList,
                    color,
                    ref linkIndex,
                    headingScale,
                    wrapWidth
                );
            }

            y += line.Height + lineSpacing;
        }

        // Reset the cursor to the captured origin before reserving layout space: link and image
        // tokens move the cursor, so when the last drawn token is a link or image the live cursor
        // is no longer at origin.
        ImGui.SetCursorScreenPos(origin);

        // Reserve the space the text occupied so following blocks flow beneath it.
        ImGui.Dummy(new Vector2(wrapWidth, y));
    }

    private static float Overhang
    (
        MarkdownFontRole role,
        float            headingScale,
        MarkdownConfig   config
    )
    {
        using ScopedMarkdownFont font = new(role, headingScale, config);
        return MarkdownTextDraw.Overhang(role, font.PixelSize);
    }

    private static Vector2 Measure
    (
        string         text,
        InlineRun      run,
        MarkdownConfig config,
        float          headingScale,
        float          availableWidth
    )
    {
        if (run.IsImage)
        {
            var image = config.ResolveImage(run);

            return image.HasValue ?
                       ResolveImageSize(image.Value.Size,                                          run, availableWidth, config, headingScale) :
                       ResolveImageSize(new Vector2(IMAGE_PLACEHOLDER_WIDTH, ImGui.GetFontSize()), run, availableWidth, config, headingScale);
        }

        // Measure with the role's font pushed so the measured width matches what DrawToken draws. The
        // italic overhang and bold stroke are not added per token: CJK is laid out one character per
        // token, so charging each of them the overhang would space the glyphs out. InlineLayout adds
        // it once at the end of the run instead.
        using ScopedMarkdownFont font = new(run.Role, headingScale, config);

        var baseWidth = ImGui.CalcTextSizeA(font.Font, font.PixelSize, float.MaxValue, 0.0F, text, out _).X;
        return new Vector2(baseWidth, font.PixelSize);
    }

    private static Vector2 ResolveImageSize
    (
        Vector2        intrinsicSize,
        InlineRun      run,
        float          availableWidth,
        MarkdownConfig config,
        float          headingScale
    )
    {
        if (run.ImageSize is { } requested)
        {
            using ScopedMarkdownFont font = new(MarkdownFontRole.Body, headingScale, config);
            var scale = run.ImageInEms ?
                            font.PixelSize :
                            GlobalUIScale;
            if (requested is { X: > 0.0F, Y: > 0.0F })
                intrinsicSize = requested * scale;
            else if (requested.X > 0.0F)
                intrinsicSize *= requested.X * scale / intrinsicSize.X;
            else if (requested.Y > 0.0F)
                intrinsicSize *= requested.Y * scale / intrinsicSize.Y;
        }

        if (intrinsicSize.X <= availableWidth)
            return intrinsicSize;

        return intrinsicSize * (availableWidth / intrinsicSize.X);
    }

    private static void DrawToken
    (
        LaidOutToken   token,
        Vector2        pos,
        MarkdownConfig config,
        ImDrawListPtr  drawList,
        uint           color,
        ref int        linkIndex,
        float          headingScale,
        float          availableWidth
    )
    {
        var role = token.Role;

        if (token.IsImage)
        {
            var image = config.ResolveImage(token.Style);

            if (image.HasValue)
            {
                ImGui.SetCursorScreenPos(pos);
                var size = ResolveImageSize(image.Value.Size, token.Style, availableWidth, config, headingScale);
                ImGui.Image(image.Value.TextureID, size);

                if (token.Style.Dispel)
                {
                    var barSize = size with { Y = MathF.Max(2.0F, size.Y * 0.08F) };
                    drawList.AddRectFilled(pos, pos + barSize, ImGui.GetColorU32(new Vector4(0.65F, 0.85F, 1.0F, 1.0F)));
                }

                Interact(token, pos, size, config, ref linkIndex);
            }
            else
            {
                // Placeholder box with the src/alt text for remote or unresolved images.
                var size = ImGui.GetFontSize();
                drawList.AddRect(pos, pos + new Vector2(token.Width, size), MarkdownColors.Separator());
                drawList.AddText(pos      + new Vector2(2.0F,        0.0F), color, token.Text);
            }

            return;
        }

        using ScopedMarkdownFont font = new(role, headingScale, config);

        var lineHeight = font.PixelSize;

        if (token.Style.Highlight)
            drawList.AddRectFilled(pos, pos + new Vector2(token.Width, lineHeight), ImGui.GetColorU32(new Vector4(0.8F, 0.65F, 0.1F, 0.35F)), 2.0F);

        if (role == MarkdownFontRole.Code)
        {
            // Subtle background behind inline code.
            Vector2 pad = new(2.0F, 1.0F);
            drawList.AddRectFilled(pos - pad, pos + new Vector2(token.Width, lineHeight) + pad, MarkdownColors.InlineCodeBackground(), 2.0F);
        }

        MarkdownTextDraw.Draw(drawList, font.Font, lineHeight, pos, color, token.Text, font.FauxBold, font.FauxItalic);

        if (token.LinkURL is not null || token.Style.Underline)
        {
            // Underline and hit-test the link token.
            var underlineY = pos.Y + lineHeight;
            drawList.AddLine(pos with { Y = underlineY }, pos with { X = pos.X + token.Width, Y = underlineY }, color, 1.0F);

        }

        if (token.Style.Strikethrough)
        {
            var strikeY = pos.Y + (lineHeight * 0.5F);
            drawList.AddLine(pos with { Y = strikeY }, new Vector2(pos.X + token.Width, strikeY), color);
        }

        Interact(token, pos, new Vector2(token.Width, lineHeight), config, ref linkIndex);
    }

    private static void Interact
    (
        LaidOutToken   token,
        Vector2        position,
        Vector2        size,
        MarkdownConfig config,
        ref int        index
    )
    {
        if (token.LinkURL is null && string.IsNullOrEmpty(token.Style.Tooltip))
            return;

        ImGui.SetCursorScreenPos(position);
        using var id = ImRaii.PushId(index++);
        ImGui.InvisibleButton("##mdinline", size);

        if (ImGui.IsItemHovered())
        {
            if (token.LinkURL is not null)
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (!string.IsNullOrEmpty(token.Style.Tooltip))
                ImGui.SetTooltip(token.Style.Tooltip);
        }

        if (token.LinkURL is not null && ImGui.IsItemClicked())
            LinkPolicy.Activate(config.ResolveURL(token.LinkURL), config.OnLinkClicked);
    }

    #region 常量

    private const float IMAGE_PLACEHOLDER_WIDTH = 120.0F;

    #endregion
}

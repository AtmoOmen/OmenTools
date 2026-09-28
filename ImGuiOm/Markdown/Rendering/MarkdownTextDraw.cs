using System.Numerics;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

// Draws text that has to look bold or italic without a second font face. Bold re-strokes the glyphs at
// a sub-pixel offset, which thickens the outline the way emboldening does; italic shears the generated
// vertices, so the glyph atlas and the texture the sampler reads stay the ones already loaded.
internal static class MarkdownTextDraw
{
    public static float BoldOffset
    (
        float pixelSize
    ) => pixelSize * BOLD_OFFSET_RATIO;

    public static float Overhang
    (
        MarkdownFontRole role,
        float            pixelSize
    )
    {
        var overhang = 0.0F;

        if (IsItalic(role))
            overhang += pixelSize * OBLIQUE_SHEAR;

        if (IsBold(role))
            overhang += MathF.Max(0.35F, BoldOffset(pixelSize));

        return overhang;
    }

    public static bool IsBold
    (
        MarkdownFontRole role
    ) => role is MarkdownFontRole.Bold or MarkdownFontRole.BoldItalic;

    public static bool IsItalic
    (
        MarkdownFontRole role
    ) => role is MarkdownFontRole.Italic or MarkdownFontRole.BoldItalic;

    public static MarkdownFontRole EmphasisRole
    (
        bool bold,
        bool italic
    ) => (bold, italic) switch
    {
        (true, true)  => MarkdownFontRole.BoldItalic,
        (true, false) => MarkdownFontRole.Bold,
        (false, true) => MarkdownFontRole.Italic,
        _             => MarkdownFontRole.Body
    };

    public static void Draw
    (
        ImDrawListPtr drawList,
        ImFontPtr     font,
        float         pixelSize,
        Vector2       position,
        uint          color,
        string        text,
        bool          bold,
        bool          italic
    )
    {
        var vertexStart = drawList.VtxBuffer.Size;

        drawList.AddText(font, pixelSize, position, color, text);

        if (bold)
        {
            // A quarter-pixel step reads as a thicker stroke, while a full pixel would look smeared.
            var offset = MathF.Max(0.35F, BoldOffset(pixelSize));
            drawList.AddText(font, pixelSize, position + new Vector2(offset, 0.0F), color, text);
        }

        if (italic)
            ShearVertices(drawList, vertexStart, position, pixelSize);
    }

    private static unsafe void ShearVertices
    (
        ImDrawListPtr drawList,
        int           vertexStart,
        Vector2       position,
        float         pixelSize
    )
    {
        // Shift each vertex in proportion to its height above the baseline, which is the same
        // transformation an oblique face applies to the outlines at rasterization time.
        var baselineY = position.Y + pixelSize;
        var vertices  = drawList.VtxBuffer;
        var data      = vertices.Data;

        for (var i = vertexStart; i < vertices.Size; i++)
            data[i].Pos.X += (baselineY - data[i].Pos.Y) * OBLIQUE_SHEAR;
    }

    #region 常量

    private const float BOLD_OFFSET_RATIO = 0.03F;
    private const float OBLIQUE_SHEAR     = 0.22F;

    #endregion
}

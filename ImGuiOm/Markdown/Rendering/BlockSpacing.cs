using OmenTools.ImGuiOm.Markdown.Parsing;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

// Distances are expressed in half line heights, so the rhythm follows the user's font size and UI scale
// instead of a fixed pixel count. The gap depends on the pair of blocks meeting, so a heading gets a
// wide gap above it and a tight one below, which visually groups it with what follows.
internal static class BlockSpacing
{
    public static float Gap
    (
        MarkdownBlockKind previous,
        MarkdownBlockKind next
    )
    {
        if (previous == MarkdownBlockKind.ThematicBreak || next == MarkdownBlockKind.ThematicBreak)
            return RULE_GAP;

        // Decided before the list and object rules: a heading opening a section needs its wide lead-in no
        // matter what kind of block precedes it, and those rules would otherwise answer first.
        if (IsHeading(next))
            return next is MarkdownBlockKind.Heading1 or MarkdownBlockKind.Heading2 ?
                       SECTION_GAP :
                       SUBHEADING_GAP;

        if (next == MarkdownBlockKind.ListItem || previous == MarkdownBlockKind.ListItem)
            return LIST_ITEM_GAP;

        if (IsWideObject(next) || IsWideObject(previous))
            return WIDE_OBJECT_GAP;

        if (IsObject(next) || IsObject(previous))
            return OBJECT_GAP;

        if (next == MarkdownBlockKind.List)
            return IsHeading(previous) ?
                       ListAfterHeadingGap(previous) :
                       LIST_INTRO_GAP;

        if (previous == MarkdownBlockKind.List)
            return LIST_OUTRO_GAP;

        if (IsHeading(previous))
            return previous is MarkdownBlockKind.Heading1 or MarkdownBlockKind.Heading2 ?
                       HEADING_GAP :
                       SUBHEADING_BIND_GAP;

        return PARAGRAPH_GAP;
    }

    private static float ListAfterHeadingGap
    (
        MarkdownBlockKind heading
    ) => heading is MarkdownBlockKind.Heading1 or MarkdownBlockKind.Heading2 ?
             HEADING_GAP :
             SUBHEADING_BIND_GAP;

    private static bool IsObject
    (
        MarkdownBlockKind kind
    ) => kind is MarkdownBlockKind.Quote
             or MarkdownBlockKind.Code
             or MarkdownBlockKind.Table
             or MarkdownBlockKind.Image;

    private static bool IsWideObject
    (
        MarkdownBlockKind kind
    ) => kind is MarkdownBlockKind.Table or MarkdownBlockKind.Image;

    private static bool IsHeading
    (
        MarkdownBlockKind kind
    ) => kind is MarkdownBlockKind.Heading1
             or MarkdownBlockKind.Heading2
             or MarkdownBlockKind.Heading3
             or MarkdownBlockKind.Heading4
             or MarkdownBlockKind.Heading5
             or MarkdownBlockKind.Heading6;

    #region 常量

    private const float PARAGRAPH_GAP       = 0.4F;
    private const float SECTION_GAP         = 1.4F;
    private const float HEADING_GAP         = 0.3F;
    private const float SUBHEADING_GAP      = 1.0F;
    private const float SUBHEADING_BIND_GAP = 0.25F;
    private const float LIST_INTRO_GAP      = 0.3F;
    private const float LIST_OUTRO_GAP      = 0.45F;
    private const float LIST_ITEM_GAP       = 0.15F;
    private const float OBJECT_GAP          = 0.5F;
    private const float WIDE_OBJECT_GAP     = 0.55F;
    private const float RULE_GAP            = 1.0F;

    #endregion
}

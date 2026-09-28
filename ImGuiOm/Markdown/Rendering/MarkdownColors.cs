using System.Numerics;

namespace OmenTools.ImGuiOm.Markdown.Rendering;

internal static class MarkdownColors
{
    public static Vector4 Link
    (
        MarkdownConfig config
    ) =>
        config.LinkColor ??
        // ButtonHovered reads as an interactive accent across the bundled themes.
        ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered];

    public static uint InlineCodeBackground() => ImGui.GetColorU32(ImGuiCol.FrameBg);

    public static uint BlockquoteBar() => ImGui.GetColorU32(ImGuiCol.Border);

    public static uint Separator() => ImGui.GetColorU32(ImGuiCol.Separator);

    public static uint TextU32() => ImGui.GetColorU32(ImGuiCol.Text);
}

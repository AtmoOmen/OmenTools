using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void ClickToCopy
    (
        string           textToCopy,
        ImGuiMouseButton mouseButton = ImGuiMouseButton.Right,
        ImGuiKey?        key         = null,
        ImGuiKey?        keyConflict = null
    )
    {
        var shouldCopy = ImGui.IsItemClicked(mouseButton)                        &&
                         (key         == null || ImGui.IsKeyDown((ImGuiKey)key)) &&
                         (keyConflict == null || !ImGui.IsKeyDown((ImGuiKey)keyConflict));

        if (shouldCopy)
            ImGui.SetClipboardText(textToCopy);
    }

    public static void CenterAlignFor(float itemWidth) =>
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X - itemWidth) / 2);
    
    private static float GetSingleLineHeight() =>
        ImGui.GetFrameHeight();

    private static float GetDoubleLineHeight() =>
        ImGui.GetFrameHeight() * 2 + ImGui.GetStyle().ItemSpacing.Y;

    private static (Vector2 Min, Vector2 Size) GetItemRect()
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        return (min, max - min);
    }

    private static (Vector2 Min, Vector2 Size) GetButtonContentRect()
    {
        var (min, size) = GetItemRect();
        var padding     = ImGui.GetStyle().FramePadding;
        var contentSize = new Vector2
        (
            MathF.Max(size.X - padding.X * 2, 0),
            MathF.Max(size.Y - padding.Y * 2, 0)
        );

        return (min + padding, contentSize);
    }

    private static Vector2 GetCenteredPosition(Vector2 areaMin, Vector2 areaSize, Vector2 contentSize)
    {
        var offset = new Vector2
        (
            MathF.Max((areaSize.X - contentSize.X) * 0.5f, 0),
            MathF.Max((areaSize.Y - contentSize.Y) * 0.5f, 0)
        );

        return areaMin + offset;
    }

    private static (Vector2 Leading, Vector2 Trailing) GetHorizontalLayout
    (
        Vector2 areaMin,
        Vector2 areaSize,
        Vector2 leadingSize,
        Vector2 trailingSize,
        float   spacing
    )
    {
        var totalSize = new Vector2(leadingSize.X + trailingSize.X + spacing, MathF.Max(leadingSize.Y, trailingSize.Y));
        var start     = GetCenteredPosition(areaMin, areaSize, totalSize);
        var leading   = start with { Y = areaMin.Y + MathF.Max((areaSize.Y - leadingSize.Y) * 0.5f, 0) };
        var trailing  = new Vector2(start.X + leadingSize.X + spacing, areaMin.Y + MathF.Max((areaSize.Y - trailingSize.Y) * 0.5f, 0));

        return (leading, trailing);
    }

    private static (Vector2 Top, Vector2 Bottom) GetVerticalLayout
    (
        Vector2 areaMin,
        Vector2 areaSize,
        Vector2 topSize,
        Vector2 bottomSize,
        float   spacing
    )
    {
        var totalSize = new Vector2(MathF.Max(topSize.X, bottomSize.X), topSize.Y + bottomSize.Y + spacing);
        var start     = GetCenteredPosition(areaMin, areaSize, totalSize);
        var top       = start with { X = areaMin.X + MathF.Max((areaSize.X - topSize.X) * 0.5f, 0) };
        var bottom    = new Vector2(areaMin.X + MathF.Max((areaSize.X - bottomSize.X) * 0.5f, 0), start.Y + topSize.Y + spacing);

        return (top, bottom);
    }

    private static Vector2 CalcIconSize(string iconText, bool useStaticFont)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont, useStaticFont);
        return ImGui.CalcTextSize(iconText);
    }

    private static void DrawIconText(Vector2 position, string iconText, bool useStaticFont)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont, useStaticFont);
        ImGui.GetWindowDrawList().AddText(position, ImGui.GetColorU32(ImGuiCol.Text), iconText);
    }
}

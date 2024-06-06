namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool Selectable(string text, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, selected, flags);
        return result;
    }

    public static bool Selectable(string text, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, ref selected, flags);
        return result;
    }

    /// <summary>
    ///     Draw a selectable that is able to fill a table cell. Not available for ImGuiSelectableFlags.SpanAllColumns
    /// </summary>
    public static bool SelectableFillCell(string text, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = cursorPos with { Y = cursorPos.Y + padding };
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    /// <summary>
    ///     Draw a selectable that is able to fill a table cell. Not available for ImGuiSelectableFlags.SpanAllColumns
    /// </summary>
    public static bool SelectableFillCell(string text, ref bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", ref selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = cursorPos with { Y = cursorPos.Y + padding };
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static bool SelectableCentered(string text, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize(text).X / 2);
        return ImGui.Selectable(text, selected, flags, size);
    }

    public static bool SelectableCentered(string text, ref bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize(text).X / 2);
        var result = ImGui.Selectable(text, ref selected, flags, size);
        return result;
    }

    public static bool SelectableImageWithText(IntPtr imageHandle, Vector2 imageSize, string text, bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{imageHandle}_{text}_{imageSize}");

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(text);
        var totalHeight = Math.Max(imageSize.Y, textSize.Y);
        var selectableSize = new Vector2(ImGui.GetContentRegionAvail().X, totalHeight);

        var result = ImGui.Selectable("", selected, flags, selectableSize);

        var imagePos = new Vector2(cursorPos.X, cursorPos.Y + (totalHeight - imageSize.Y) / 2 + 2.5f);
        windowDrawList.AddImage(imageHandle, imagePos, new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y));

        var textPos = new Vector2(cursorPos.X + imageSize.X + ImGui.GetStyle().ItemSpacing.X,
            cursorPos.Y + (totalHeight - textSize.Y) / 2 + 2.5f);

        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool SelectableImageWithText(IntPtr imageHandle, Vector2 imageSize, string text, ref bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{imageHandle}_{text}_{imageSize}");

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(text);
        var totalHeight = Math.Max(imageSize.Y, textSize.Y);
        var selectableSize = new Vector2(ImGui.GetContentRegionAvail().X, totalHeight);

        var result = ImGui.Selectable("", ref selected, flags, selectableSize);

        var imagePos = new Vector2(cursorPos.X, cursorPos.Y + (totalHeight - imageSize.Y) / 2 + 2.5f);
        windowDrawList.AddImage(imageHandle, imagePos, new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y));

        var textPos = new Vector2(cursorPos.X + imageSize.X + ImGui.GetStyle().ItemSpacing.X,
            cursorPos.Y + (totalHeight - textSize.Y) / 2 + 2.5f);

        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool SelectableTextCentered(string text, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{text}_{flags}");
        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = new Vector2(cursorPos.X + (selectableWidth - textSize.X) / 2, cursorPos.Y + padding);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();

        return result;
    }

    public static bool SelectableTextCentered(string text, ref bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{text}_{flags}");
        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", ref selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = new Vector2(cursorPos.X + (selectableWidth - textSize.X) / 2, cursorPos.Y + padding);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();

        return result;
    }

    public static bool SelectableIconCentered(string id, FontAwesomeIcon icon, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{icon}_{id}");
        ImGui.PushFont(UiBuilder.IconFont);
        var textSize = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = new Vector2(cursorPos.X + (selectableWidth - textSize.X + padding) / 2, cursorPos.Y + padding);
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        ImGui.PopID();

        return result;
    }

    public static bool SelectableIconCentered(string id, FontAwesomeIcon icon, ref bool selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{icon}_{id}");
        ImGui.PushFont(UiBuilder.IconFont);
        var textSize = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var padding = ImGui.GetStyle().FramePadding.X;
        var selectableWidth = ImGui.GetContentRegionAvail().X;
        var selectableHeight = textSize.Y + 2 * padding;

        var result = ImGui.Selectable("", ref selected, flags, new Vector2(selectableWidth, selectableHeight));

        var textPos = new Vector2(cursorPos.X + (selectableWidth - textSize.X + padding) / 2, cursorPos.Y + padding);
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        ImGui.PopFont();
        ImGui.PopID();

        return result;
    }
}
namespace OmenTools.ImGuiOm;

public static class Selectable
{
    public static bool SelectableCentered(string text, bool selected = false, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize(text).X / 2);
        return ImGui.Selectable(text, selected, flags, size);
    }

    public static bool SelectableCentered(string text, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize(text).X / 2);
        var result = ImGui.Selectable(text, ref selected, flags, size);
        return result;
    }

    public static bool SelectableImageWithText(IntPtr imageHandle, Vector2 imageSize, string text, bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{imageHandle}_{text}_{imageSize}");

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(text);
        var totalHeight = Math.Max(imageSize.Y, textSize.Y);
        var selectableSize = new Vector2(ImGui.GetContentRegionAvail().X, totalHeight);

        var result = ImGui.Selectable("", selected, flags, selectableSize);

        var imagePos = new Vector2(cursorPos.X, cursorPos.Y + ((totalHeight - imageSize.Y) / 2) + 2.5f);
        windowDrawList.AddImage(imageHandle, imagePos, new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y));

        var textPos = new Vector2(cursorPos.X + imageSize.X + ImGui.GetStyle().ItemSpacing.X, cursorPos.Y + ((totalHeight - textSize.Y) / 2) + 2.5f);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }

    public static bool SelectableImageWithText(IntPtr imageHandle, Vector2 imageSize, string text, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{imageHandle}_{text}_{imageSize}");

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(text);
        var totalHeight = Math.Max(imageSize.Y, textSize.Y);
        var selectableSize = new Vector2(ImGui.GetContentRegionAvail().X, totalHeight);

        var result = ImGui.Selectable("", ref selected, flags, selectableSize);

        var imagePos = new Vector2(cursorPos.X, cursorPos.Y + ((totalHeight - imageSize.Y) / 2) + 2.5f);
        windowDrawList.AddImage(imageHandle, imagePos, new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y));

        var textPos = new Vector2(cursorPos.X + imageSize.X + ImGui.GetStyle().ItemSpacing.X, cursorPos.Y + ((totalHeight - textSize.Y) / 2) + 2.5f);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        ImGui.PopID();

        return result;
    }
}
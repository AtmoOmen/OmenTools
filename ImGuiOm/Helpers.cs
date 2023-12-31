﻿namespace OmenTools.ImGuiOm;

public class Helpers
{
    public static void ClickToCopy(string textToCopy, ImGuiMouseButton mouseButton = ImGuiMouseButton.Right, ImGuiKey? key = null, ImGuiKey? keyConflict = null)
    {
        var shouldCopy = ImGui.IsItemClicked(mouseButton) &&
                          (key == null || ImGui.IsKeyDown((ImGuiKey)key)) &&
                          (keyConflict == null || !ImGui.IsKeyDown((ImGuiKey)keyConflict));

        if (shouldCopy)
            ImGui.SetClipboardText(textToCopy);
    }

    public static void CenterAlignFor(int itemWidth)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - itemWidth / 2);
    }
}
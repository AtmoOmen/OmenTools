﻿using System.Numerics;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool TreeNodeImageWithText(nint image, Vector2 imageSize, string text, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
    {
        var spaceCount  = (int)MathF.Ceiling(imageSize.X / ImGui.CalcTextSize(" ").X);
        var spacingText = new string(' ', spaceCount);
        
        var startCursorPos = ImGui.GetCursorPos();
        var isOpen         = ImGui.TreeNodeEx($"{spacingText} {text}", flags);
        
        ImGui.SameLine();
        ImGui.SetCursorPosX(startCursorPos.X + ImGui.GetTreeNodeToLabelSpacing());
        ImGui.Image(image, imageSize);
        
        return isOpen;
    }
}

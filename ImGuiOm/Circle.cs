using System.Numerics;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    private static void CircleOutlined(
        Vector2 center, float radius, uint fillColor, uint outlineColor = 0xFF000000, float outlineThickness = 1.5f,
        ImDrawListPtr? drawList = null)
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        // 描边
        drawList?.AddCircleFilled(center, radius + outlineThickness, outlineColor);
        // 原始
        drawList?.AddCircleFilled(center, radius, fillColor);
    }
}

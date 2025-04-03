using System.Numerics;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void CircleOutlined(
        Vector2 center, float radius, uint fillColor, uint outlineColor = 0xFF000000, float outlineThickness = 1.5f,
        float opacity = 1f, ImDrawListPtr? drawList = null)
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        // 不透明度
        var fillAlpha = (byte)(((fillColor >> 24) & 0xFF) * opacity);
        var outlineAlpha = (byte)(((outlineColor >> 24) & 0xFF) * opacity);
        var fillColorWithOpacity = (fillColor & 0x00FFFFFF) | ((uint)fillAlpha << 24);
        var outlineColorWithOpacity = (outlineColor & 0x00FFFFFF) | ((uint)outlineAlpha << 24);

        // 描边
        drawList?.AddCircleFilled(center, radius + outlineThickness, outlineColorWithOpacity);
        // 原始
        drawList?.AddCircleFilled(center, radius, fillColorWithOpacity);
    }
}

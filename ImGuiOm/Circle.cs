using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void CircleOutlined(
        Vector2        center,
        float          radius,
        Vector4        fillColor,
        Vector4        outlineColor     = default,
        float          outlineThickness = 1.5f,
        float          opacity          = 1f,
        ImDrawListPtr? drawList         = null)
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        if (outlineColor == default)
            outlineColor = new Vector4(0, 0, 0, 1);

        // 不透明度
        var fillColorWithOpacity    = fillColor with { W = fillColor.W       * opacity };
        var outlineColorWithOpacity = outlineColor with { W = outlineColor.W * opacity };

        // 描边
        drawList?.AddCircleFilled(center, radius + outlineThickness, ImGui.ColorConvertFloat4ToU32(outlineColorWithOpacity));
        // 原始
        drawList?.AddCircleFilled(center, radius, ImGui.ColorConvertFloat4ToU32(fillColorWithOpacity));
    }
}

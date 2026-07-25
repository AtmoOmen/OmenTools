using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void CircleOutlined
    (
        Vector2        center,
        float          radius,
        uint           fillColor,
        uint           outlineColor     = 0xFF000000,
        float          outlineThickness = 1.5f,
        float          opacity          = 1f,
        ImDrawListPtr? drawList         = null
    )
    {
        drawList ??= ImGui.GetBackgroundDrawList();

        var fillColorVector    = fillColor.ToVector4();
        var outlineColorVector = outlineColor.ToVector4();
        var fillColorWithOpacity    = fillColorVector with { W = fillColorVector.W * opacity };
        var outlineColorWithOpacity = outlineColorVector with { W = outlineColorVector.W * opacity };

        drawList?.AddCircleFilled(center, radius + outlineThickness, outlineColorWithOpacity.ToUInt());
        drawList?.AddCircleFilled(center, radius, fillColorWithOpacity.ToUInt());
    }

    public static void CircleOutlined
    (
        Vector2        center,
        float          radius,
        Vector4        fillColor,
        Vector4?       outlineColor     = null,
        float          outlineThickness = 1.5f,
        float          opacity          = 1f,
        ImDrawListPtr? drawList         = null
    ) =>
        CircleOutlined
        (
            center,
            radius,
            fillColor.ToUInt(),
            outlineColor?.ToUInt() ?? 0xFF000000,
            outlineThickness,
            opacity,
            drawList
        );
}

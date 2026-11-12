using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>在指定绘制列表中绘制带描边的实心圆。</summary>
    /// <param name="center">圆心的屏幕坐标。</param>
    /// <param name="radius">填充区域半径。</param>
    /// <param name="fillColor">填充的打包颜色。</param>
    /// <param name="outlineColor">描边的打包颜色。</param>
    /// <param name="outlineThickness">描边向外扩展的厚度。</param>
    /// <param name="opacity">填充和描边的不透明度乘数。</param>
    /// <param name="drawList">目标绘制列表；未指定时使用背景绘制列表。</param>
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
        var target = drawList ?? ImGui.GetBackgroundDrawList();

        var fillColorVector         = fillColor.ToVector4();
        var outlineColorVector      = outlineColor.ToVector4();
        var fillColorWithOpacity    = fillColorVector with { W = fillColorVector.W       * opacity };
        var outlineColorWithOpacity = outlineColorVector with { W = outlineColorVector.W * opacity };

        target.AddCircleFilled(center, radius + outlineThickness, outlineColorWithOpacity.ToUInt());
        target.AddCircleFilled(center, radius,                    fillColorWithOpacity.ToUInt());
    }

    /// <summary>使用向量颜色绘制带描边的实心圆。</summary>
    /// <param name="center">圆心的屏幕坐标。</param>
    /// <param name="radius">填充区域半径。</param>
    /// <param name="fillColor">填充颜色。</param>
    /// <param name="outlineColor">描边颜色；未指定时使用黑色。</param>
    /// <param name="outlineThickness">描边向外扩展的厚度。</param>
    /// <param name="opacity">填充和描边的不透明度乘数。</param>
    /// <param name="drawList">目标绘制列表；未指定时使用背景绘制列表。</param>
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

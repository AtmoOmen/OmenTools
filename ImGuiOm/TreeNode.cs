using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>绘制包含图片和文本的树节点，保留原生树节点的展开和缩进状态。</summary>
    /// <param name="image">图片纹理句柄。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    /// <param name="text">节点标签；双井号后的部分仅参与标识。</param>
    /// <param name="flags">原生树节点标志。</param>
    /// <returns>节点处于展开状态时返回真。</returns>
    public static bool TreeNodeImageWithText(nint image, Vector2 imageSize, string text, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None) =>
        TreeNodeImageWithText(new ImTextureID(image), imageSize, text, flags);

    /// <summary>使用纹理标识绘制图文树节点，按内容高度计算节点内边距。</summary>
    /// <param name="image">图片纹理标识。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    /// <param name="text">节点标签；双井号后的部分仅参与标识。</param>
    /// <param name="flags">原生树节点标志。</param>
    /// <returns>节点处于展开状态时返回真。</returns>
    public static bool TreeNodeImageWithText(ImTextureID image, Vector2 imageSize, string text, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
    {
        var       displaySpan  = GetDisplaySpan(text);
        var       metrics      = TextMetrics.Measure(displaySpan);
        var       style        = ImGui.GetStyle();
        var       height       = MathF.Max(imageSize.Y, MathF.Max(metrics.Size.Y, metrics.LineAdvance));
        var       padding      = style.FramePadding with { Y = MathF.Max(style.FramePadding.Y, (height - metrics.LineAdvance) / 2f) };
        using var paddingScope = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, padding);

        var origin        = ImGui.GetCursorScreenPos();
        var labelSpacing  = ImGui.GetTreeNodeToLabelSpacing();
        var isOpen        = ImGui.TreeNodeEx($"##{text}", flags | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding);
        var nodeHeight    = ImGui.GetItemRectSize().Y;
        var imagePosition = origin + new Vector2(labelSpacing,                                          (nodeHeight - imageSize.Y) / 2f);
        var textOrigin    = origin + new Vector2(labelSpacing + imageSize.X + style.ItemInnerSpacing.X, 0);
        var textPosition  = metrics.CenterIn(textOrigin, metrics.Size with { Y = nodeHeight });
        var drawList      = ImGui.GetWindowDrawList();

        drawList.AddImage(image, imagePosition, imagePosition + imageSize, Vector2.Zero, Vector2.One, ImGui.GetColorU32(Vector4.One));
        drawList.AddText(textPosition, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return isOpen;
    }
}

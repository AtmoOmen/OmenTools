using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>按全局界面比例提交正方形布局占位。</summary>
    /// <param name="x">缩放前的正方形边长。</param>
    public static void ScaledDummy(float x) =>
        ImGui.Dummy(new Vector2(x) * GlobalUIScale);

    /// <summary>按全局界面比例提交矩形布局占位。</summary>
    /// <param name="x">缩放前的宽度。</param>
    /// <param name="y">缩放前的高度。</param>
    public static void ScaledDummy(float x, float y) =>
        ImGui.Dummy(new Vector2(x, y) * GlobalUIScale);
}

using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void ScaledDummy(float x) =>
        ImGui.Dummy(new Vector2(x) * GlobalUIScale);

    public static void ScaledDummy(float x, float y) =>
        ImGui.Dummy(new Vector2(x, y) * GlobalUIScale);
}

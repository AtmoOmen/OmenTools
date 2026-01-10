using System.Numerics;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static Vector2 ScaledVector2(float x) => 
        new Vector2(x) * GlobalFontScale;

    public static Vector2 ScaledVector2(float x, float y) => 
        new Vector2(x, y) * GlobalFontScale;

    public static Vector2 ScaledVector2(Vector2 size) =>
        size * GlobalFontScale;

    public static void ScaledDummy(float x) => 
        ImGui.Dummy(new Vector2(x) * GlobalFontScale);

    public static void ScaledDummy(float x, float y) => 
        ImGui.Dummy(new Vector2(x, y) * GlobalFontScale);
}

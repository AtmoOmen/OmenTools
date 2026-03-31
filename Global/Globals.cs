using System.Numerics;
using OmenTools.OmenService;

namespace OmenTools.Global;

public static class Globals
{
    public static float GlobalUIScale => FontManager.Instance().GlobalFontScale;

    public static Vector2 ScaledVector2(float x) =>
        new Vector2(x) * GlobalUIScale;

    public static Vector2 ScaledVector2(float x, float y) =>
        new Vector2(x, y) * GlobalUIScale;

    public static Vector2 ScaledVector2(Vector2 size) =>
        size * GlobalUIScale;
}

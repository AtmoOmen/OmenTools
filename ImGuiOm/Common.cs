using System.Numerics;

namespace OmenTools.ImGuiOm;

// 暂时不太好分类的
public static partial class ImGuiOm
{
    public static Vector4 GetGradientColor()
    {
        const float period = 1f;
        
        var t     = (float)ImGui.GetTime() % period                 / period;
        var red   = (MathF.Sin(2 * MathF.PI * t)               + 1) / 2;
        var green = (MathF.Sin(2 * MathF.PI * (t + (1f / 3f))) + 1) / 2;
        var blue  = (MathF.Sin(2 * MathF.PI * (t + (2f / 3f))) + 1) / 2;

        return new Vector4(red, green, blue, 1f);
    }
}

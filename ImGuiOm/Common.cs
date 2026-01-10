using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;

namespace OmenTools.ImGuiOm;

// 暂时不太好分类的
public static partial class ImGuiOm
{
    private static readonly Dictionary<(string PlayerName, string WorldName), byte[]> RenderedPlayerInfos = [];

    public static Vector4 GetGradientColor()
    {
        const float PERIOD = 1f;

        var t     = (float)ImGui.GetTime() % PERIOD               / PERIOD;
        var red   = (MathF.Sin(2 * MathF.PI * t)             + 1) / 2;
        var green = (MathF.Sin(2 * MathF.PI * (t + 1f / 3f)) + 1) / 2;
        var blue  = (MathF.Sin(2 * MathF.PI * (t + 2f / 3f)) + 1) / 2;

        return new Vector4(red, green, blue, 1f);
    }

    public static void RenderPlayerInfo(string name, string world)
    {
        using var group = ImRaii.Group();

        ImGuiHelpers.SeStringWrapped
        (
            RenderedPlayerInfos.GetOrAdd
            (
                (name, world),
                _ => new SeStringBuilder()
                     .AddText($"{name}")
                     .AddIcon(BitmapFontIcon.CrossWorld)
                     .AddText($"{world}")
                     .Encode()
            )
        );
    }
}

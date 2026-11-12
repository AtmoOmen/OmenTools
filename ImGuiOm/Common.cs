using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using Lumina.Text.ReadOnly;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    private static readonly Dictionary<(string PlayerName, string WorldName), ReadOnlySeString> RenderedPlayerInfos = [];

    /// <summary>按 ImGui 时间计算循环变化的彩虹色。</summary>
    /// <returns>当前时刻的打包颜色。</returns>
    public static uint GetGradientColor()
    {
        var t     = (float)ImGui.GetTime() % PERIOD                 / PERIOD;
        var red   = (MathF.Sin(2 * MathF.PI * t)               + 1) / 2;
        var green = (MathF.Sin(2 * MathF.PI * (t + (1f / 3f))) + 1) / 2;
        var blue  = (MathF.Sin(2 * MathF.PI * (t + (2f / 3f))) + 1) / 2;

        return new Vector4(red, green, blue, 1f).ToUInt();
    }

    /// <summary>绘制玩家名称、跨服图标与服务器名称组成的富文本。</summary>
    /// <param name="name">玩家名称。</param>
    /// <param name="world">服务器名称。</param>
    public static void RenderPlayerInfo(string name, string world)
    {
        using var group = ImRaii.Group();

        ImGuiHelpers.SeStringWrapped
        (
            RenderedPlayerInfos.GetOrAdd
            (
                (name, world),
                _ =>
                {
                    using var rented = new RentedSeStringBuilder();

                    return rented.Builder
                                 .Append($"{name}")
                                 .AppendIcon((uint)BitmapFontIcon.CrossWorld)
                                 .Append($"{world}")
                                 .ToReadOnlySeString();
                }
            )
        );
    }

    /// <summary>使用逐层扩展的半透明矩形绘制外发光效果。</summary>
    /// <param name="drawList">目标绘制列表。</param>
    /// <param name="min">矩形左上角。</param>
    /// <param name="max">矩形右下角。</param>
    /// <param name="colU32">发光的打包颜色。</param>
    /// <param name="rounding">矩形圆角半径。</param>
    /// <param name="glowSize">发光向外扩展的距离。</param>
    /// <param name="steps">发光层数。</param>
    /// <param name="innerBoost">内层发光强度的额外增量。</param>
    public static void AddGlowRect
    (
        ImDrawListPtr drawList,
        Vector2       min,
        Vector2       max,
        uint          colU32,
        float         rounding,
        float         glowSize,
        int           steps,
        float         innerBoost = 0.25f
    )
    {
        var baseColor = colU32.ToVector4();

        for (var index = 0; index < steps; index++)
        {
            var t      = (index + 1f) / steps;
            var expand = t            * glowSize;

            var a = 1f - t;
            a *= a;
            a *= 1f / steps * 2.2f;

            a *= 1f + ((1f - t) * innerBoost);

            var color = baseColor with { W = baseColor.W * a };

            var r = rounding           + (expand * 0.6f);
            drawList.AddRectFilled(min - new Vector2(expand), max + new Vector2(expand), color.ToUInt(), r);
        }
    }

    #region 常量

    private const float PERIOD = 1f;

    #endregion
}

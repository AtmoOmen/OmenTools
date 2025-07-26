using System.Buffers.Binary;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using ImGuiNET;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static ByteColor ConvertVector4ToByteColor(Vector4 color)
    {
        var r = (byte)Math.Round(Math.Clamp(color.X, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
        var g = (byte)Math.Round(Math.Clamp(color.Y, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
        var b = (byte)Math.Round(Math.Clamp(color.Z, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
        var a = (byte)Math.Round(Math.Clamp(color.W, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);

        return new ByteColor { R = r, G = g, B = b, A = a };
    }
    
    public static Vector4 ConvertByteColorToVector4(ByteColor color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;
        var a = color.A / 255f;

        return new Vector4(r, g, b, a);
    }
    
    public static Vector4 UIColorToVector4Color(uint uiColorRowColor)
        => ImGui.ColorConvertU32ToFloat4(UIColorToU32Color(uiColorRowColor));
    
    public static uint UIColorToU32Color(uint uiColorRowColor)
        => BinaryPrimitives.ReverseEndianness(uiColorRowColor) | 0xFF000000u;
    
    public static Vector4 HexToVector4(string hexColor, bool includeAlpha = true)
    {
        if (!hexColor.StartsWith('#')) throw new ArgumentException("Invalid hex color format");

        hexColor = hexColor[1..];

        int r, g, b, a;
        switch (hexColor.Length)
        {
            case 3:
                r = Convert.ToInt32(hexColor.Substring(0, 1), 16) * 17;
                g = Convert.ToInt32(hexColor.Substring(1, 1), 16) * 17;
                b = Convert.ToInt32(hexColor.Substring(2, 1), 16) * 17;
                a = includeAlpha ? 255 : 0;
                break;
            case 4:
                r = Convert.ToInt32(hexColor.Substring(0, 1), 16) * 17;
                g = Convert.ToInt32(hexColor.Substring(1, 1), 16) * 17;
                b = Convert.ToInt32(hexColor.Substring(2, 1), 16) * 17;
                a = Convert.ToInt32(hexColor.Substring(3, 1), 16) * 17;
                break;
            case 6:
                r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                a = includeAlpha ? 255 : 0;
                break;
            case 8:
                r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                a = Convert.ToInt32(hexColor.Substring(6, 2), 16);
                break;
            default:
                throw new ArgumentException("Invalid hex color length");
        }

        return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    public static Vector4 DarkenColor(Vector4 originalColor, float darkenAmount)
    {
        darkenAmount = Math.Clamp(darkenAmount, 0f, 1f);

        var newR = Math.Max(0, originalColor.X - (originalColor.X * darkenAmount));
        var newG = Math.Max(0, originalColor.Y - (originalColor.Y * darkenAmount));
        var newB = Math.Max(0, originalColor.Z - (originalColor.Z * darkenAmount));

        return new Vector4(newR, newG, newB, originalColor.W);
    }
    
    public static Vector4 LightenColor(Vector4 originalColor, float lightenAmount)
    {
        lightenAmount = Math.Clamp(lightenAmount, 0f, 1f);

        var newR = Math.Min(1, originalColor.X + (1 - originalColor.X) * lightenAmount);
        var newG = Math.Min(1, originalColor.Y + (1 - originalColor.Y) * lightenAmount);
        var newB = Math.Min(1, originalColor.Z + (1 - originalColor.Z) * lightenAmount);

        return new Vector4(newR, newG, newB, originalColor.W);
    }
}

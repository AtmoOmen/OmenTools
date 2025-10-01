using System.Numerics;
using System.Runtime.CompilerServices;

namespace OmenTools.Helpers;

public static class ColorExtension
{
    private static readonly Dictionary<KnownColor, Vector4> KnownColorToVector4 = [];
    private static readonly Dictionary<uint, Vector4>       UIntToVector4       = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this KnownColor knownColor) =>
        KnownColorToVector4.GetOrAdd(knownColor, _ =>
        {
            var rgbColor = Color.FromKnownColor(knownColor);
            return new Vector4(rgbColor.R, rgbColor.G, rgbColor.B, rgbColor.A) / 255.0f;
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt(this Vector4 color) => 
        ImGui.ColorConvertFloat4ToU32(color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this uint color) => 
        UIntToVector4.GetOrAdd(color, _ => ImGui.ColorConvertU32ToFloat4(color));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this Vector4 color) => 
        new(color.X, color.Y, color.Z);

    public static Vector4 Invert(this Vector4 v) => 
        v with { X = 1f - v.X, Y = 1f - v.Y, Z = 1f - v.Z };
}

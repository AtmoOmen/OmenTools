using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class ColorExtension
{
    private static readonly Dictionary<ImGuiCol, Vector4>   ImGuiColToVector4   = [];
    private static readonly Dictionary<ImGuiCol, uint>      ImGuiColToUInt      = [];
    private static readonly Dictionary<KnownColor, Vector4> KnownColorToVector4 = [];
    private static readonly Dictionary<KnownColor, uint>    KnownColorToUInt    = [];
    private static readonly Dictionary<uint, Vector4>       UIntToVector4       = [];

    extension(ImGuiCol imguiCol)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() =>
            ImGuiColToVector4.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol).ToVector4());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt() =>
            ImGuiColToUInt.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol));
    }

    extension(KnownColor knownColor)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() =>
            KnownColorToVector4.GetOrAdd(knownColor, _ => knownColor.Vector());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt() =>
            KnownColorToUInt.GetOrAdd(knownColor, _ => knownColor.ToVector4().ToUInt());
    }

    extension(uint color)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() =>
            UIntToVector4.GetOrAdd(color, _ => ImGui.ColorConvertU32ToFloat4(color));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetVector4UIColor() =>
            AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color).ToVector4();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUIntUIColor() =>
            AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color);
    }

    extension(scoped in Vector4 color)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3() =>
            new(color.X, color.Y, color.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 Invert() =>
            color with { X = 1f - color.X, Y = 1f - color.Y, Z = 1f - color.Z };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteColor ToByteColor()
        {
            var r = (byte)Math.Round(Math.Clamp(color.X, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
            var g = (byte)Math.Round(Math.Clamp(color.Y, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
            var b = (byte)Math.Round(Math.Clamp(color.Z, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
            var a = (byte)Math.Round(Math.Clamp(color.W, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);

            return new ByteColor { R = r, G = g, B = b, A = a };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt() =>
            ImGui.ColorConvertFloat4ToU32(color);
    }

    extension(ByteColor color)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            var r = color.R / 255f;
            var g = color.G / 255f;
            var b = color.B / 255f;
            var a = color.A / 255f;

            return new Vector4(r, g, b, a);
        }
    }
}

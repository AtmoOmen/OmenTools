using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Dalamud.DataShare.Attributes;

namespace OmenTools.Extensions;

public static unsafe class ColorExtension
{
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
        public Vector4 ReverseToVector4() =>
            ReverseUIntToVector4.GetOrAdd(color, _ =>
                {
                    var processed = ImGui.ColorConvertU32ToFloat4(color);
                    processed = new Vector4(processed.Z, processed.Y, processed.X, 1f);
                    return processed;
                }
            );

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
    
    #region 数据
    
    [DataShareTag]
    private const string IMGUI_COL_TO_VECTOR4_TAG = "OmenTools.Extensions.ColorExtension.ImGuiColToVector4";

    private static readonly Dictionary<ImGuiCol, Vector4> ImGuiColToVector4 =
        IDalamudPluginInterface.Instance().GetOrCreateData(IMGUI_COL_TO_VECTOR4_TAG, static () => new Dictionary<ImGuiCol, Vector4>());

    [DataShareTag]
    private const string IMGUI_COL_TO_UINT_TAG = "OmenTools.Extensions.ColorExtension.ImGuiColToUInt";

    private static readonly Dictionary<ImGuiCol, uint> ImGuiColToUInt =
        IDalamudPluginInterface.Instance().GetOrCreateData(IMGUI_COL_TO_UINT_TAG, static () => new Dictionary<ImGuiCol, uint>());

    [DataShareTag]
    private const string KNOWN_COLOR_TO_VECTOR4_TAG = "OmenTools.Extensions.ColorExtension.KnownColorToVector4";

    private static readonly Dictionary<KnownColor, Vector4> KnownColorToVector4 =
        IDalamudPluginInterface.Instance().GetOrCreateData(KNOWN_COLOR_TO_VECTOR4_TAG, static () => new Dictionary<KnownColor, Vector4>());

    [DataShareTag]
    private const string KNOWN_COLOR_TO_UINT_TAG = "OmenTools.Extensions.ColorExtension.KnownColorToUInt";

    private static readonly Dictionary<KnownColor, uint> KnownColorToUInt =
        IDalamudPluginInterface.Instance().GetOrCreateData(KNOWN_COLOR_TO_UINT_TAG, static () => new Dictionary<KnownColor, uint>());

    [DataShareTag]
    private const string UINT_TO_VECTOR4_TAG = "OmenTools.Extensions.ColorExtension.UIntToVector4";

    private static readonly Dictionary<uint, Vector4> UIntToVector4 =
        IDalamudPluginInterface.Instance().GetOrCreateData(UINT_TO_VECTOR4_TAG, static () => new Dictionary<uint, Vector4>());

    [DataShareTag]
    private const string REVERSE_UINT_TO_VECTOR4_TAG = "OmenTools.Extensions.ColorExtension.ReverseUIntToVector4";

    private static readonly Dictionary<uint, Vector4> ReverseUIntToVector4 =
        IDalamudPluginInterface.Instance().GetOrCreateData(REVERSE_UINT_TO_VECTOR4_TAG, static () => new Dictionary<uint, Vector4>());
    
    #endregion
}

using System.Numerics;
using System.Runtime.CompilerServices;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    private const float PACKET_SCALE     = 10430.2195f;
    private const float INV_PACKET_SCALE = 1 / PACKET_SCALE;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float WorldDirHToCharaRotation(Vector2 direction) =>
        MathF.Atan2(direction.X, direction.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CharaRotationSymmetricTransform(float rotation) =>
        rotation - MathF.CopySign(MathF.PI, rotation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CameraDirHToCharaRotation(float cameraDirH)
    {
        var result = (cameraDirH + MathF.PI) % MathF.Tau;
        return result < 0 ? result + MathF.Tau : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRotationChanged(float target, float current, float tolerance = 0.1f)
    {
        const float TWO_PI = 2 * MathF.PI;
        var         diff   = MathF.Abs(current - target);

        if (diff > MathF.PI)
            diff = TWO_PI - diff;

        return diff > tolerance;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort CharaRotationToPacketRotation(float rotation)
    {
        var transformed = (rotation + MathF.PI) * PACKET_SCALE;
        return (ushort)Math.Clamp(transformed, 0f, 65535f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PacketRotationToCharaRotation(ushort rotation) =>
        rotation * INV_PACKET_SCALE - MathF.PI;
}

using System.Numerics;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static float WorldDirHToCharaRotation(Vector2 direction) 
        => direction == Vector2.Zero ? 0f : MathF.Atan2(direction.X, direction.Y);

    public static float CharaRotationSymmetricTransform(float rotation) 
        => MathF.IEEERemainder(rotation + MathF.PI, 2 * MathF.PI);
    
    public static float CameraDirHToCharaRotation(float cameraDirH)
        => (cameraDirH - MathF.PI) % (2 * MathF.PI);

    public static ushort CharaRotationToPacketRotation(float rotation)
    {
        var transformed = (ushort)((rotation + 3.1415927f) * 100.0f / 0.0095875263f);
        return (ushort)(transformed < 65535 ? transformed : 65535);
    }
    
    public static float PacketRotationToCharaRotation(ushort rotation)
    {
        var scaledValue   = rotation    * 0.0095875263f;
        var adjustedValue = scaledValue / 100.0f;
        var originalValue = adjustedValue - 3.1415927f;

        return (float)Math.Atan2(Math.Sin(originalValue), Math.Cos(originalValue));
    }
}

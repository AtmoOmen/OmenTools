using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace OmenTools.Infos;

public readonly struct Angle(float radians)
{
    public const float RAD_TO_DEG = 180      / MathF.PI;
    public const float DEG_TO_RAD = MathF.PI / 180;

    public float Rad { get; init; } = radians;
    public float Deg => Rad * RAD_TO_DEG;

    public static Angle FromDirection(Vector2 dir) => new(MathF.Atan2(dir.X, dir.Y));

    public static Angle FromDirectionXZ(Vector3 dir) => new(MathF.Atan2(dir.X, dir.Z));
    
    public static unsafe bool TryGetDirectionToDestination(
        Vector3                desiredPosition, 
        bool                   allowVertical, 
        out (Angle H, Angle V) direction, 
        float                  precision = 0.1f)
    {
        direction = default;
    
        if (DService.ObjectTable.LocalPlayer is not { } localPlayer) 
            return false;

        var dist = desiredPosition - localPlayer.Position;
        if (dist.LengthSquared() <= precision * precision) 
            return false;

        var dirH = FromDirectionXZ(dist);
        var dirV = allowVertical
                       ? FromDirection(new(dist.Y, new Vector2(dist.X, dist.Z).Length()))
                       : default;

        var refDir = DService.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1
                         ? new Angle(((CameraEx*)CameraManager.Instance()->GetActiveCamera())->DirH) + new Angle(180 * DEG_TO_RAD)
                         : new Angle(localPlayer.Rotation);

        direction = (dirH - refDir, dirV);
        return true;
    }

    public Vector2 ToDirection() => new(MathF.Sin(Rad), MathF.Cos(Rad));

    public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);

    public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);

    public override string ToString() => Deg.ToString("f0");

    public override bool Equals(object? obj) => obj is Angle angle && Rad == angle.Rad;

    public override int GetHashCode() => Rad.GetHashCode();

    public static bool operator ==(Angle l, Angle r) => l.Rad == r.Rad;

    public static bool operator !=(Angle l, Angle r) => l.Rad != r.Rad;
}


using System.Numerics;
using CSVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;

namespace OmenTools.Extensions;

public static class VectorExtension
{
    extension(scoped in Vector3 vector3)
    {
        public Vector2 ToVector2() =>
            new(vector3.X, vector3.Z);
    }

    extension(scoped in CSVector3 vector3)
    {
        public Vector2 ToVector2() =>
            new(vector3.X, vector3.Z);
    }

    extension(scoped in Vector2 vector2)
    {
        public Vector3 ToVector3(scoped in float y) =>
            new(vector2.X, y, vector2.Y);

        public Vector2 Scale() =>
            vector2 * GlobalFontScale;
    }
}

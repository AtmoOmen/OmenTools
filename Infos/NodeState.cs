using System.Numerics;
using Dalamud.Utility.Numerics;

namespace OmenTools.Infos;

public readonly record struct NodeState(
    Vector2 TopLeft,
    Vector2 Size,
    bool    Visible)
{
    public float Width  => Size.X;
    public float Height => Size.Y;

    public Vector2 Center => TopLeft + Size / 2;

    public Vector2 TopRight    => TopLeft + Size.WithY(0);
    public Vector2 BottomLeft  => TopLeft + Size.WithX(0);
    public Vector2 BottomRight => TopLeft + Size;

    public float X => TopLeft.X;
    public float Y => TopLeft.Y;
}

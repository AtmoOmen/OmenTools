using System.Numerics;
using Dalamud.Utility.Numerics;

namespace OmenTools.Infos;

public readonly record struct NodeState(
    Vector2 TopLeft,
    Vector2 Size,
    bool    Visible)
{
    public float   Height      { get; } = Size.Y;
    public float   Width       { get; } = Size.X;
    
    public Vector2 Center      { get; } = TopLeft + (Size / 2);
    
    public Vector2 TopRight    { get; } = TopLeft + Size.WithY(0);
    public Vector2 BottomLeft  { get; } = TopLeft + Size.WithX(0);
    public Vector2 BottomRight { get; } = TopLeft + Size;

    public float X { get; } = TopLeft.X;
    public float Y { get; } = TopLeft.Y;
}

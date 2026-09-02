using System.Numerics;

namespace OmenTools.Interop.Game.Models;

public record AddonPosition
(
    Vector2                Position,
    AddonPositionAlignment Alignment = AddonPositionAlignment.TopLeft
);

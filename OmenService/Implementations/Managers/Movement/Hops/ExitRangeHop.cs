using System.Numerics;

namespace OmenTools.OmenService;

public sealed record ExitRangeHop
(
    uint    TargetZone,
    Vector3 Position
) : ZoneHop(TargetZone);

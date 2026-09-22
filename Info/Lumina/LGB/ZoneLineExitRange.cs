using System.Numerics;

namespace OmenTools.Info.Lumina.LGB;

public readonly record struct ZoneLineExitRange
(
    uint    SourceZone,
    uint    TargetZone,
    Vector3 Position
);

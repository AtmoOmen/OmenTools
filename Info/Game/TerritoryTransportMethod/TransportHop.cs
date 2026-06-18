using OmenTools.Info.Game.TerritoryTransportMethod.Abstractions;

namespace OmenTools.Info.Game.TerritoryTransportMethod;

public sealed record TransportHop
(
    TerritoryTransportMethodBase Method,
    uint                         TargetTerritory
);

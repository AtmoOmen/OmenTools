using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.OmenService;

public sealed record EventHop
(
    EventTransportBase Method,
    uint               TargetZone
) : ZoneHop(TargetZone);

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RisingStonesToMorDhona : EventSimpleYesTransportBase
{
    public override string DisplayName => "石之家 → 摩杜纳";

    public override uint SourceZone => 351;

    public override IReadOnlyList<uint> TargetZones => [156];

    public override Vector3 EventPosition => new(0.2f, 2.0f, 24.9f);

    protected override uint EventID => 131145;
}

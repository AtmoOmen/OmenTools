using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class MorDhonaToRisingStones : EventSimpleYesTransportBase
{
    public override string DisplayName => "摩杜纳 → 石之家";

    public override uint SourceZone => 156;

    public override IReadOnlyList<uint> TargetZones => [351];

    public override Vector3 EventPosition => new(22.3f, 21.3f, -633.7f);

    protected override uint EventID => 131144;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class WakingSandsToWesternThanalan : EventSimpleYesTransportBase
{
    public override string DisplayName => "沙之家 → 西萨纳兰";

    public override uint SourceZone => 212;

    public override IReadOnlyList<uint> TargetZones => [140];

    public override Vector3 EventPosition => new(-13.5f, 0.0f, 0.2f);

    protected override uint EventID => 131089;
}

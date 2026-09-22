using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class SyrcusTrenchToMorDhona : EventSimpleYesTransportBase
{
    public override string DisplayName => "希尔科斯峡谷 → 摩杜纳";

    public override uint SourceZone => 842;

    public override IReadOnlyList<uint> TargetZones => [156];

    public override Vector3 EventPosition => new(-70.0f, 4.9f, 77.8f);

    protected override uint EventID => 131323;
}

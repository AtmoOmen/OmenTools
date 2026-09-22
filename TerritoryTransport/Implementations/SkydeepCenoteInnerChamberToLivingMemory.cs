using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class SkydeepCenoteInnerChamberToLivingMemory : EventSimpleYesTransportBase
{
    public override string DisplayName => "深空天坑最深处 → 活着的记忆";

    public override uint SourceZone => 1222;

    public override IReadOnlyList<uint> TargetZones => [1192];

    public override Vector3 EventPosition => new(0.0f, 0.2f, -84.5f);

    protected override uint EventID => 131561;
}

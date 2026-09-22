using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LivingMemoryToSkydeepCenoteInnerChamber : EventSimpleYesTransportBase
{
    public override string DisplayName => "活着的记忆 → 深空天坑最深处";

    public override uint SourceZone => 1192;

    public override IReadOnlyList<uint> TargetZones => [1222];

    public override Vector3 EventPosition => new(-0.1f, 106.2f, 863.6f);

    protected override uint EventID => 131560;
}

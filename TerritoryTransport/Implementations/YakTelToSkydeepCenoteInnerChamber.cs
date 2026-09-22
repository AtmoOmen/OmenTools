using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class YakTelToSkydeepCenoteInnerChamber : EventSimpleYesTransportBase
{
    public override string DisplayName => "亚克特尔树海 → 深空天坑最深处";

    public override uint SourceZone => 1189;

    public override IReadOnlyList<uint> TargetZones => [1222];

    public override Vector3 EventPosition => new(-826.2f, -297.9f, 874.5f);

    protected override uint EventID => 131553;
}

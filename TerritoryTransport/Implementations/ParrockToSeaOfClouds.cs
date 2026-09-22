using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ParrockToSeaOfClouds : EventSimpleYesTransportBase
{
    public override string DisplayName => "帕洛克系留基地 → 阿巴拉提亚云海";

    public override uint SourceZone => 567;

    public override IReadOnlyList<uint> TargetZones => [401];

    public override Vector3 EventPosition => new(44.3f, -15.5f, 19.1f);

    protected override uint EventID => 131224;
}

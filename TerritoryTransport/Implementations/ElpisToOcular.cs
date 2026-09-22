using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ElpisToOcular : EventSimpleYesTransportBase
{
    public override string DisplayName => "厄尔庇斯 → 观星室";

    public override uint SourceZone => 961;

    public override IReadOnlyList<uint> TargetZones => [844];

    public override Vector3 EventPosition => new(281.3f, 47.2f, 832.2f);

    protected override uint EventID => 131452;
}

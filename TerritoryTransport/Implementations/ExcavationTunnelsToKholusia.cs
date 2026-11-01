using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ExcavationTunnelsToKholusia : EventSimpleYesTransportBase
{
    public override string DisplayName => "机械遗迹坑道 → 珂露西亚岛";

    public override uint SourceZone => 895;

    public override IReadOnlyList<uint> TargetZones => [814];

    public override Vector3 EventPosition => new(1.7f, 0.0f, 0.9f);

    protected override uint EventID => 131348;
}

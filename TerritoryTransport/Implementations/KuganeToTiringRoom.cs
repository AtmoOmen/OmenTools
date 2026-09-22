using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KuganeToTiringRoom : EventSimpleYesTransportBase
{
    public override string DisplayName => "黄金港 → 剧场艇初见号道具间";

    public override uint SourceZone => 628;

    public override IReadOnlyList<uint> TargetZones => [735];

    public override Vector3 EventPosition => new(-56.7f, 79.1f, 47.5f);

    protected override uint EventID => 131281;
}

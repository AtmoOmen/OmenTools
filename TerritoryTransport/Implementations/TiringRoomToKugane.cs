using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class TiringRoomToKugane : EventSimpleYesTransportBase
{
    public override string DisplayName => "剧场艇初见号道具间 → 黄金港";

    public override uint SourceZone => 735;

    public override IReadOnlyList<uint> TargetZones => [628];

    public override Vector3 EventPosition => new(-26.3f, -0.8f, -0.2f);

    protected override uint EventID => 131282;
}

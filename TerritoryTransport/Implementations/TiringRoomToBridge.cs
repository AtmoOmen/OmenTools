using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class TiringRoomToBridge : EventSimpleYesTransportBase
{
    public override string DisplayName => "剧场艇初见号道具间 → 剧场艇初见号舰桥";

    public override uint SourceZone => 735;

    public override IReadOnlyList<uint> TargetZones => [736];

    public override Vector3 EventPosition => new(36.6f, 0.4f, 0.0f);

    protected override uint EventID => 131283;
}

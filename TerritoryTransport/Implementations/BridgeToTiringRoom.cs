using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class BridgeToTiringRoom : EventSimpleYesTransportBase
{
    public override string DisplayName => "剧场艇初见号舰桥 → 剧场艇初见号道具间";

    public override uint SourceZone => 736;

    public override IReadOnlyList<uint> TargetZones => [735];

    public override Vector3 EventPosition => new(7.3f, -0.0f, 0.1f);

    protected override uint EventID => 131284;
}

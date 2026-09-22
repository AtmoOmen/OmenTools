using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class CoerthasCentralHighlandsToFoundation : EventSimpleYesTransportBase
{
    public override string DisplayName =>
        "库尔札斯中央高地 → 伊修加德基础层";

    public override uint SourceZone => 155;

    public override IReadOnlyList<uint> TargetZones => [418];

    public override Vector3 EventPosition => new(-162.1f, 304.2f, -322.4f);

    protected override uint EventID => 131185;
}

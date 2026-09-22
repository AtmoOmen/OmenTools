using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class FoundationToCoerthasCentralHighlands : EventSimpleYesTransportBase
{
    public override string DisplayName =>
        "伊修加德基础层 → 库尔札斯中央高地";

    public override uint SourceZone => 418;

    public override IReadOnlyList<uint> TargetZones => [155];

    public override Vector3 EventPosition => new(-0.8f, -2.6f, 145.1f);

    protected override uint EventID => 131186;
}

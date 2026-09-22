using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class YakTelToLowerJeuno : EventSimpleYesTransportBase
{
    public override string DisplayName => "亚克特尔树海 → 朱诺下层";

    public override uint SourceZone => 1189;

    public override IReadOnlyList<uint> TargetZones => [1265];

    public override Vector3 EventPosition => new(-525.0f, -152.5f, 671.6f);

    protected override uint EventID => 131584;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LowerJeunoToYakTel : EventSimpleYesTransportBase
{
    public override string DisplayName => "朱诺下层 → 亚克特尔树海";

    public override uint SourceZone => 1265;

    public override IReadOnlyList<uint> TargetZones => [1189];

    public override Vector3 EventPosition => new(-0.1f, 42.0f, 11.8f);

    protected override uint EventID => 131583;
}

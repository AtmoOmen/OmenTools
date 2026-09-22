using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class SkydeepCenoteInnerChamberToYakTel : EventSimpleYesTransportBase
{
    public override string DisplayName => "深空天坑最深处 → 亚克特尔树海";

    public override uint SourceZone => 1222;

    public override IReadOnlyList<uint> TargetZones => [1189];

    public override Vector3 EventPosition => new(0.1f, -0.0f, -10.8f);

    protected override uint EventID => 131554;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class FringesToEastShroud : EventSimpleYesTransportBase
{
    public override string DisplayName => "基拉巴尼亚边区 → 黑衣森林东部林区";

    public override uint SourceZone => 612;

    public override IReadOnlyList<uint> TargetZones => [152];

    public override Vector3 EventPosition => new(-662.0f, 130.0f, -532.3f);

    protected override uint EventID => 131250;
}

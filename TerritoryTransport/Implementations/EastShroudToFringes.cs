using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class EastShroudToFringes : EventSimpleYesTransportBase
{
    public override string DisplayName => "黑衣森林东部林区 → 基拉巴尼亚边区";

    public override uint SourceZone => 152;

    public override IReadOnlyList<uint> TargetZones => [612];

    public override Vector3 EventPosition => new(25.0f, 5.8f, 403.9f);

    protected override uint EventID => 131251;
}

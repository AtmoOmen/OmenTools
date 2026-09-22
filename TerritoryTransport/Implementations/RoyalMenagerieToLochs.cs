using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RoyalMenagerieToLochs : EventSimpleYesTransportBase
{
    public override string DisplayName => "空中庭园 → 基拉巴尼亚湖区";

    public override uint SourceZone => 740;

    public override IReadOnlyList<uint> TargetZones => [621];

    public override Vector3 EventPosition => new(-466.3f, 383.0f, -126.9f);

    protected override uint EventID => 131288;
}

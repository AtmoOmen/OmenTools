using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LochsToRoyalMenagerie : EventSimpleYesTransportBase
{
    public override string DisplayName => "基拉巴尼亚湖区 → 空中庭园";

    public override uint SourceZone => 621;

    public override IReadOnlyList<uint> TargetZones => [740];

    public override Vector3 EventPosition => new(746.0f, 70.0f, 530.4f);

    protected override uint EventID => 131443;
}

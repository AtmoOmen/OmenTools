using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GarlemaldToLochs : EventSimpleYesTransportBase
{
    public override string DisplayName => "加雷马 → 基拉巴尼亚湖区";

    public override uint SourceZone => 958;

    public override IReadOnlyList<uint> TargetZones => [621];

    public override Vector3 EventPosition => new(-493.2f, 27.6f, 632.6f);

    protected override uint EventID => 131419;
}

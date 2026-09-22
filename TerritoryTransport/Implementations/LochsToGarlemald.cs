using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LochsToGarlemald : EventSimpleYesTransportBase
{
    public override string DisplayName => "基拉巴尼亚湖区 → 加雷马";

    public override uint SourceZone => 621;

    public override IReadOnlyList<uint> TargetZones => [958];

    public override Vector3 EventPosition => new(744.5f, 70.0f, 536.0f);

    protected override uint EventID => 131420;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class StepsOfNaldToNewGridania : EventSimpleYesTransportBase
{
    public override string DisplayName => "乌尔达哈现世回廊 → 格里达尼亚新街";

    public override uint SourceZone => 130;

    public override IReadOnlyList<uint> TargetZones => [132];

    public override Vector3 EventPosition => new(-23.1f, 83.2f, -6.0f);

    protected override uint EventID => 131101;
    
    public override uint Cost => 120;
}

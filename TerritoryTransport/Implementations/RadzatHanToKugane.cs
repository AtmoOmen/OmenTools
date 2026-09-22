using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RadzatHanToKugane : EventSimpleYesTransportBase
{
    public override string DisplayName => "拉札罕 → 黄金港";

    public override uint SourceZone => 963;

    public override IReadOnlyList<uint> TargetZones => [628];

    public override Vector3 EventPosition => new(-142.3f, 28.0f, 229.3f);

    protected override uint EventID => 131399;
    
    public override uint Cost => 300;
}

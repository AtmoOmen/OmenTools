using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KuganeToRadzatHan : EventSimpleYesTransportBase
{
    public override string DisplayName => "黄金港 → 拉札罕";

    public override uint SourceZone => 628;

    public override IReadOnlyList<uint> TargetZones => [963];

    public override Vector3 EventPosition => new(-63.4f, 79.1f, 48.6f);

    protected override uint EventID => 131400;
    
    public override uint Cost => 300;
}

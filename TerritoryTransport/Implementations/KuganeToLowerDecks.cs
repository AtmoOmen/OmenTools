using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KuganeToLowerDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "黄金港 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone => 628;

    public override IReadOnlyList<uint> TargetZones => [129];

    public override Vector3 EventPosition => new(-101.1f, -7.0f, -62.3f);

    protected override uint EventID => 131252;
    
    public override uint Cost => 300;
}

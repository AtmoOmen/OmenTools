using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class WesternLaNosceaToLowerDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "西拉诺西亚 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone => 138;

    public override IReadOnlyList<uint> TargetZones => [129];

    public override Vector3 EventPosition => new(317.7f, -36.3f, 350.2f);

    protected override uint EventID => 131110;
    
    public override uint Cost => 40;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class WesternThanalanToLowerDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "西萨纳兰 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone => 140;

    public override IReadOnlyList<uint> TargetZones => [129];

    public override Vector3 EventPosition => new(-487.8f, 24.0f, -330.7f);

    protected override uint EventID => 131107;
    
    public override uint Cost => 80;
}

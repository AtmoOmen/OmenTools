using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OldSharlayanToLowerDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "旧萨雷安 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone => 962;

    public override IReadOnlyList<uint> TargetZones => [129];

    public override Vector3 EventPosition => new(155.2f, -16.1f, 180.5f);

    protected override uint EventID => 131405;
    
    public override uint Cost => 300;
}

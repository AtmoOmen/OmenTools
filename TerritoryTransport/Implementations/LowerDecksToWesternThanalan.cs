using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LowerDecksToWesternThanalan : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨下层甲板 → 西萨纳兰";

    public override uint SourceZone => 129;

    public override IReadOnlyList<uint> TargetZones => [140];

    public override Vector3 EventPosition => new(-359.7f, 8.0f, 42.8f);

    protected override uint EventID => 131108;
    
    public override uint Cost => 80;
}

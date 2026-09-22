using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LowerDecksToEasternLaNoscea : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨下层甲板 → 东拉诺西亚";

    public override uint SourceZone => 129;

    public override IReadOnlyList<uint> TargetZones => [137];

    public override Vector3 EventPosition => new(-189.1f, 1.0f, 208.7f);

    protected override uint EventID => 131111;
    
    public override uint Cost => 40;
}

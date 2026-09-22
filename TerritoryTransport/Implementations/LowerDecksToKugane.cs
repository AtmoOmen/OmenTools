using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LowerDecksToKugane : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨下层甲板 → 黄金港";

    public override uint SourceZone => 129;

    public override IReadOnlyList<uint> TargetZones => [628];

    public override Vector3 EventPosition => new(-353.4f, 8.0f, 46.1f);

    protected override uint EventID => 131253;
    
    public override uint Cost => 300;
}

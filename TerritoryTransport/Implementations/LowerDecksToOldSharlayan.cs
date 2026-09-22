using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LowerDecksToOldSharlayan : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨下层甲板 → 旧萨雷安";

    public override uint SourceZone => 129;

    public override IReadOnlyList<uint> TargetZones => [962];

    public override Vector3 EventPosition => new(-387.2f, 6.0f, 44.1f);

    protected override uint EventID => 131406;
    
    public override uint Cost => 300;
}

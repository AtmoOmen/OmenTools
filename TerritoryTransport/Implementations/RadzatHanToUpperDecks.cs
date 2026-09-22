using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RadzatHanToUpperDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "拉札罕 → 利姆萨·罗敏萨上层甲板";

    public override uint SourceZone => 963;

    public override IReadOnlyList<uint> TargetZones => [128];

    public override Vector3 EventPosition => new(-142.3f, 28.0f, 229.3f);

    protected override uint EventID => 131433;
    
    public override uint Cost => 300;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class UpperDecksToGoldSaucer : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨上层甲板 → 金碟游乐场";

    public override uint SourceZone => 128;

    public override IReadOnlyList<uint> TargetZones => [144];

    public override Vector3 EventPosition => new(-22.3f, 92.0f, -3.8f);

    protected override uint EventID => 131179;
    
    public override uint Cost => 120;
}

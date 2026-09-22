using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class UpperDecksToNewGridania : EventSimpleYesTransportBase
{
    public override string DisplayName => "利姆萨·罗敏萨上层甲板 → 格里达尼亚新街";

    public override uint SourceZone => 128;

    public override IReadOnlyList<uint> TargetZones => [132];

    public override Vector3 EventPosition => new(-22.3f, 92.0f, -3.8f);

    protected override uint EventID => 131106;
    
    public override uint Cost => 120;
}

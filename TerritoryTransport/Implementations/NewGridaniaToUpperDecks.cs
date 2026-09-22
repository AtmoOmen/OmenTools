using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class NewGridaniaToUpperDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "格里达尼亚新街 → 利姆萨·罗敏萨上层甲板";

    public override uint SourceZone => 132;

    public override IReadOnlyList<uint> TargetZones => [128];

    public override Vector3 EventPosition => new(28.7f, -19.0f, 101.0f);

    protected override uint EventID => 131104;
    
    public override uint Cost => 120;
}

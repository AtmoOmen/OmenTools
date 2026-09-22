using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class PillarsToUpperDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "伊修加德砥柱层 → 利姆萨·罗敏萨上层甲板";

    public override uint SourceZone => 419;

    public override IReadOnlyList<uint> TargetZones => [128];

    public override Vector3 EventPosition => new(153.7f, -12.6f, -8.6f);

    protected override uint EventID => 131234;
    
    public override uint Cost => 150;
}

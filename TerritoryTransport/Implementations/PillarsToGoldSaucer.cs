using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class PillarsToGoldSaucer : EventSimpleYesTransportBase
{
    public override string DisplayName => "伊修加德砥柱层 → 金碟游乐场";

    public override uint SourceZone => 419;

    public override IReadOnlyList<uint> TargetZones => [144];

    public override Vector3 EventPosition => new(153.7f, -12.6f, -8.6f);

    protected override uint EventID => 131238;
    
    public override uint Cost => 150;
}

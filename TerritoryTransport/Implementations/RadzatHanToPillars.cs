using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RadzatHanToPillars : EventSimpleYesTransportBase
{
    public override string DisplayName => "拉札罕 → 伊修加德砥柱层";

    public override uint SourceZone => 963;

    public override IReadOnlyList<uint> TargetZones => [419];

    public override Vector3 EventPosition => new(-142.3f, 28.0f, 229.3f);

    protected override uint EventID => 131435;
    
    public override uint Cost => 500;
}

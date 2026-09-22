using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToPillars : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 伊修加德砥柱层";

    public override uint SourceZone => 144;

    public override IReadOnlyList<uint> TargetZones => [419];

    public override Vector3 EventPosition => new(-38.2f, 0.0f, 100.5f);

    protected override uint EventID => 131237;

    public override uint Cost => 150;
}

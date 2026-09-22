using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToStepsOfNald : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 乌尔达哈现世回廊";

    public override uint SourceZone => 144;

    public override IReadOnlyList<uint> TargetZones => [130];

    public override Vector3 EventPosition => new(-38.2f, 0.0f, 100.5f);

    protected override uint EventID => 131183;

    public override uint Cost => 0;
}

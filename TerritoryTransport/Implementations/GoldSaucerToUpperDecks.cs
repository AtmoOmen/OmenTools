using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToUpperDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 利姆萨·罗敏萨上层甲板";

    public override uint SourceZone => 144;

    public override IReadOnlyList<uint> TargetZones => [128];

    public override Vector3 EventPosition => new(-38.2f, 0.0f, 100.5f);

    protected override uint EventID => 131182;

    public override uint Cost => 120;
}

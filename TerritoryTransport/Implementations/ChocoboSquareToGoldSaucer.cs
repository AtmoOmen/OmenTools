using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ChocoboSquareToGoldSaucer : EventSimpleYesTransportBase
{
    public override string DisplayName => "陆行鸟广场 → 金碟游乐场";

    public override uint SourceZone => 388;

    public override IReadOnlyList<uint> TargetZones => [144];

    public override Vector3 EventPosition => new(-1.7f, 0.0f, 12.8f);

    protected override uint EventID => 131178;
}

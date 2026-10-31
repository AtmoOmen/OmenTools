using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToChocoboSquare : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 陆行鸟广场";

    public override uint SourceZone => 144;

    public override IReadOnlyList<uint> TargetZones => [388];

    public override Vector3 EventPosition => new(-83.4f, 0.0f, 24.8f);

    protected override uint EventID => 131177;
}

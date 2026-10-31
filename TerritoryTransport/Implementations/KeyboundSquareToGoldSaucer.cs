using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KeyboundSquareToGoldSaucer : EventSimpleYesTransportBase
{
    public override string DisplayName => "键斗广场 → 金碟游乐场";

    public override uint SourceZone => 1360;

    public override IReadOnlyList<uint> TargetZones => [144];

    public override Vector3 EventPosition => new(-0.1f, -0.0f, -6.0f);

    protected override uint EventID => 131634;
}

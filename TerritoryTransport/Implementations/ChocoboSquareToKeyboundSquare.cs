using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ChocoboSquareToKeyboundSquare : EventSimpleYesTransportBase
{
    public override string DisplayName => "陆行鸟广场 → 键斗广场";

    public override uint SourceZone => 388;

    public override IReadOnlyList<uint> TargetZones => [1360];

    public override Vector3 EventPosition => new(-2.5f, 0.0f, -32.4f);

    protected override uint EventID => 131635;
}

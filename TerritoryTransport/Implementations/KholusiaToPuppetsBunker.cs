using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KholusiaToPuppetsBunker : EventSimpleYesTransportBase
{
    public override string DisplayName => "珂露西亚岛 → 人偶军事基地";

    public override uint SourceZone => 814;

    public override IReadOnlyList<uint> TargetZones => [928];

    public override Vector3 EventPosition => new(701.8f, 293.6f, -159.1f);

    protected override uint EventID => 131373;
}

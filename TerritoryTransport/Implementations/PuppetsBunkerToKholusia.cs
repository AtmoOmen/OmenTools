using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class PuppetsBunkerToKholusia : EventSimpleYesTransportBase
{
    public override string DisplayName => "人偶军事基地 → 珂露西亚岛";

    public override uint SourceZone => 928;

    public override IReadOnlyList<uint> TargetZones => [814];

    public override Vector3 EventPosition => new(-382.8f, 102.0f, 388.1f);

    protected override uint EventID => 131374;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KholusiaToCrystarium : EventSimpleYesTransportBase
{
    public override string DisplayName => "珂露西亚岛 → 水晶都";

    public override uint SourceZone => 814;

    public override IReadOnlyList<uint> TargetZones => [819];

    public override Vector3 EventPosition => new(796.8f, 1.8f, 258.6f);

    protected override uint EventID => 131332;
}

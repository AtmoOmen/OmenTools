using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class EulmoreToCrystarium : EventSimpleYesTransportBase
{
    public override string DisplayName => "游末邦 → 水晶都";

    public override uint SourceZone => 820;

    public override IReadOnlyList<uint> TargetZones => [819];

    public override Vector3 EventPosition => new(0.0f, 84.8f, -76.9f);

    protected override uint EventID => 131355;
}

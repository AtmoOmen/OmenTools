using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OcularToCrystarium : EventSimpleYesTransportBase
{
    public override string DisplayName => "观星室 → 水晶都";

    public override uint SourceZone => 844;

    public override IReadOnlyList<uint> TargetZones => [819];

    public override Vector3 EventPosition => new(-0.1f, 0.0f, 12.4f);

    protected override uint EventID => 131311;
}

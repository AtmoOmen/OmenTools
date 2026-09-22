using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OcularToElpis : EventSimpleYesTransportBase
{
    public override string DisplayName => "观星室 → 厄尔庇斯";

    public override uint SourceZone => 844;

    public override IReadOnlyList<uint> TargetZones => [961];

    public override Vector3 EventPosition => new(-0.0f, 0.8f, -10.6f);

    protected override uint EventID => 131453;
}

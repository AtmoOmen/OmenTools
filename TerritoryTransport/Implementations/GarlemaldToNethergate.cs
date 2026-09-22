using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GarlemaldToNethergate : EventSimpleYesTransportBase
{
    public override string DisplayName => "加雷马 → 神门之间";

    public override uint SourceZone => 958;

    public override IReadOnlyList<uint> TargetZones => [1024];

    public override Vector3 EventPosition => new(-480.2f, 10.9f, -673.0f);

    protected override uint EventID => 131450;
}

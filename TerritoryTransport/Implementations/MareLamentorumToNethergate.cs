using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class MareLamentorumToNethergate : EventSimpleYesTransportBase
{
    public override string DisplayName => "叹息海 → 神门之间";

    public override uint SourceZone => 959;

    public override IReadOnlyList<uint> TargetZones => [1024];

    public override Vector3 EventPosition => new(-660.9f, 131.6f, 727.1f);

    protected override uint EventID => 131451;
}

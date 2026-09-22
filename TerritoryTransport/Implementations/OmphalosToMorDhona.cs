using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OmphalosToMorDhona : EventSimpleYesTransportBase
{
    public override string DisplayName => "翁法洛斯 → 摩杜纳";

    public override uint SourceZone => 1061;

    public override IReadOnlyList<uint> TargetZones => [156];

    public override Vector3 EventPosition => new(14.5f, 0.6f, -58.2f);

    protected override uint EventID => 131473;
}

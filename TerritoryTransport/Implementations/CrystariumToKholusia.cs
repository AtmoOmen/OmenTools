using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class CrystariumToKholusia : EventSimpleYesTransportBase
{
    public override string DisplayName => "水晶都 → 珂露西亚岛";

    public override uint SourceZone => 819;

    public override IReadOnlyList<uint> TargetZones => [814];

    public override Vector3 EventPosition => new(55.9f, 36.2f, -168.4f);

    protected override uint EventID => 131329;
}

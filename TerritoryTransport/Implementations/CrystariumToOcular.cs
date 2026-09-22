using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class CrystariumToOcular : EventSimpleYesTransportBase
{
    public override string DisplayName => "水晶都 → 观星室";

    public override uint SourceZone => 819;

    public override IReadOnlyList<uint> TargetZones => [844];

    public override Vector3 EventPosition => new(115.0f, 14.6f, 7.2f);

    protected override uint EventID => 131370;
}

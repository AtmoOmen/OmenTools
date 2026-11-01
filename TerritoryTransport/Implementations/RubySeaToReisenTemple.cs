using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class RubySeaToReisenTemple : EventSimpleYesTransportBase
{
    public override string DisplayName => "红玉海 → 醴泉神社";

    public override uint SourceZone => 613;

    public override IReadOnlyList<uint> TargetZones => [764];

    public override Vector3 EventPosition => new(282.2f, 16.2f, 842.0f);

    protected override uint EventID => 131295;
}

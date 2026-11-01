using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ReisenTempleToRubySea : EventSimpleYesTransportBase
{
    public override string DisplayName => "醴泉神社 → 红玉海";

    public override uint SourceZone => 764;

    public override IReadOnlyList<uint> TargetZones => [613];

    public override Vector3 EventPosition => new(62.6f, -90.0f, -440.3f);

    protected override uint EventID => 131297;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class DomanEnclaveToGangos : EventSimpleYesTransportBase
{
    public override string DisplayName => "多玛飞地 → 甘戈斯";

    public override uint SourceZone => 759;

    public override IReadOnlyList<uint> TargetZones => [915];

    public override Vector3 EventPosition => new(123.2f, -4.2f, 98.4f);

    protected override uint EventID => 131359;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class DravanianForelandsToChurningMists : EventSimpleYesTransportBase
{
    public override string DisplayName =>
        "龙堡参天高地 → 翻云雾海";

    public override uint SourceZone => 398;

    public override IReadOnlyList<uint> TargetZones => [400];

    public override Vector3 EventPosition => new(-693.3f, 5.0f, -833.4f);

    protected override uint EventID => 131199;
}

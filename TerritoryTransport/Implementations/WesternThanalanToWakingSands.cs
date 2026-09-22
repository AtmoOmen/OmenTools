using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class WesternThanalanToWakingSands : EventSimpleYesTransportBase
{
    public override string DisplayName => "西萨纳兰 → 沙之家";

    public override uint SourceZone => 140;

    public override IReadOnlyList<uint> TargetZones => [212];

    public override Vector3 EventPosition => new(-482.2f, 17.0f, -386.9f);

    protected override uint EventID => 131088;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class NethergateToMareLamentorum : EventSimpleYesTransportBase
{
    public override string DisplayName => "神门之间 → 叹息海";

    public override uint SourceZone => 1024;

    public override IReadOnlyList<uint> TargetZones => [959];

    public override Vector3 EventPosition => new(-0.0f, 5.6f, 2.1f);

    protected override uint EventID => 131462;
}

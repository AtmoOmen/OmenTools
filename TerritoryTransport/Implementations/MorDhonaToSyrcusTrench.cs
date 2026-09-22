using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class MorDhonaToSyrcusTrench : EventSimpleYesTransportBase
{
    public override string DisplayName => "摩杜纳 → 希尔科斯峡谷";

    public override uint SourceZone => 156;

    public override IReadOnlyList<uint> TargetZones => [842];

    public override Vector3 EventPosition => new(353.6f, -25.4f, -374.1f);

    protected override uint EventID => 131322;
}

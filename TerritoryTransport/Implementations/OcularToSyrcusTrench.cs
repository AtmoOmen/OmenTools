using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OcularToSyrcusTrench : EventSimpleYesTransportBase
{
    public override string DisplayName => "观星室 → 希尔科斯峡谷";

    public override uint SourceZone => 844;

    public override IReadOnlyList<uint> TargetZones => [842];

    public override Vector3 EventPosition => new(-0.0f, 0.9f, -11.1f);

    protected override uint EventID => 131325;
}

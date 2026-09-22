using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class SyrcusTrenchToOcular : EventSimpleYesTransportBase
{
    public override string DisplayName => "希尔科斯峡谷 → 观星室";

    public override uint SourceZone => 842;

    public override IReadOnlyList<uint> TargetZones => [844];

    public override Vector3 EventPosition => new(-48.4f, 1.4f, 40.9f);

    protected override uint EventID => 131324;
}

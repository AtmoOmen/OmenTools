using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class PhantomVillageToTuliyollal : EventSimpleYesTransportBase
{
    public override string DisplayName => "幻境村 → 图莱尤拉";

    public override uint SourceZone => 1278;

    public override IReadOnlyList<uint> TargetZones => [1185];

    public override Vector3 EventPosition => new(78.0f, -11.2f, 13.4f);

    protected override uint EventID => 131593;
}

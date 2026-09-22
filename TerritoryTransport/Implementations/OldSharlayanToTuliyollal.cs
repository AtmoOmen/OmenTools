using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OldSharlayanToTuliyollal : EventSimpleYesTransportBase
{
    public override string DisplayName => "旧萨雷安 → 图莱尤拉";

    public override uint SourceZone => 962;

    public override IReadOnlyList<uint> TargetZones => [1185];

    public override Vector3 EventPosition => new(141.1f, -16.1f, 221.8f);

    protected override uint EventID => 131543;
}

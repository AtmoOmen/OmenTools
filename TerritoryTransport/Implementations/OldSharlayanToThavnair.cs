using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OldSharlayanToThavnair : EventSimpleYesTransportBase
{
    public override string DisplayName => "旧萨雷安 → 萨维奈岛";

    public override uint SourceZone => 962;

    public override IReadOnlyList<uint> TargetZones => [957];

    public override Vector3 EventPosition => new(-80.2f, 6.6f, -39.4f);

    protected override uint EventID => 131448;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OldSharlayanToLabyrinthos : EventSimpleYesTransportBase
{
    public override string DisplayName => "旧萨雷安 → 迷津";

    public override uint SourceZone => 962;

    public override IReadOnlyList<uint> TargetZones => [956];

    public override Vector3 EventPosition => new(-53.4f, 18.2f, -320.8f);

    protected override uint EventID => 131407;
}

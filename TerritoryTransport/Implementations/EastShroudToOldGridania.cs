using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class EastShroudToOldGridania : EventSimpleYesTransportBase
{
    public override string DisplayName => "黑衣森林东部林区 → 格里达尼亚旧街";

    public override uint SourceZone => 152;

    public override IReadOnlyList<uint> TargetZones => [133];

    public override Vector3 EventPosition => new(-572.2f, 8.7f, 75.3f);

    protected override uint EventID => 131078;
}

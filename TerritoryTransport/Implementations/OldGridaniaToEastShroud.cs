using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class OldGridaniaToEastShroud : EventSimpleYesTransportBase
{
    public override string DisplayName => "格里达尼亚旧街 → 黑衣森林东部林区";

    public override uint SourceZone => 133;

    public override IReadOnlyList<uint> TargetZones => [152];

    public override Vector3 EventPosition => new(176.0f, -1.5f, -242.2f);

    protected override uint EventID => 131077;
}

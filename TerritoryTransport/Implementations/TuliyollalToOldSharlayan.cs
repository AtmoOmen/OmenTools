using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class TuliyollalToOldSharlayan : EventSimpleYesTransportBase
{
    public override string DisplayName => "图莱尤拉 → 旧萨雷安";

    public override uint SourceZone => 1185;

    public override IReadOnlyList<uint> TargetZones => [962];

    public override Vector3 EventPosition => new(123.0f, -18.0f, 142.9f);

    protected override uint EventID => 131545;
}

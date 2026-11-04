using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class KholusiaToExcavationTunnels : EventSimpleYesTransportBase
{
    public override string DisplayName => "珂露西亚岛 → 机械遗迹坑道";

    public override uint SourceZone => 814;

    public override Vector3 EventPosition => new(736.1f, 285.1f, -83.7f);

    protected override uint EventID => 131349;
}

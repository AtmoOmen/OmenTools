using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class CopiedFactoryToExcavationTunnels : EventSimpleYesTransportBase
{
    public override string DisplayName => "复制工厂废墟 → 机械遗迹坑道";

    public override uint SourceZone => 896;

    public override IReadOnlyList<uint> TargetZones => [895];

    public override Vector3 EventPosition => new(-498.8f, 484.0f, 278.5f);

    protected override uint EventID => 131351;
}

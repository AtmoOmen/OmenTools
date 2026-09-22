using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class YanxiaToDomanEnclave : EventSimpleYesTransportBase
{
    public override string DisplayName => "延夏 → 多玛飞地";

    public override uint SourceZone => 614;

    public override IReadOnlyList<uint> TargetZones => [759];

    public override Vector3 EventPosition => new(-491.3f, 1.1f, 541.1f);

    protected override uint EventID => 131293;
}

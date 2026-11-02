using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class UnnamedIslandToLowerLaNoscea : EventSimpleYesTransportBase
{
    public override string DisplayName =>
        "无名岛 → 拉诺西亚低地";
    
    public override uint SourceZone => 1055;

    public override IReadOnlyList<uint> TargetZones => [135];

    public override Vector3 EventPosition => new(-434.7f, -0.2f, 205.8f);

    protected override uint EventID => 984091;
}

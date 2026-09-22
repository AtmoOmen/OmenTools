using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GangosToDomanEnclave : EventSimpleYesTransportBase
{
    public override string DisplayName => "甘戈斯 → 多玛飞地";

    public override uint SourceZone => 915;

    public override IReadOnlyList<uint> TargetZones => [759];

    public override Vector3 EventPosition => new(-50.3f, 0.2f, -36.3f);

    protected override uint EventID => 131360;
}

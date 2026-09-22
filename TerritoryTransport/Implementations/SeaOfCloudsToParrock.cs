using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class SeaOfCloudsToParrock : EventSimpleYesTransportBase
{
    public override string DisplayName => "阿巴拉提亚云海 → 帕洛克系留基地";

    public override uint SourceZone => 401;

    public override IReadOnlyList<uint> TargetZones => [567];

    public override Vector3 EventPosition => new(-813.9f, -88.3f, -834.3f);

    protected override uint EventID => 131223;
}

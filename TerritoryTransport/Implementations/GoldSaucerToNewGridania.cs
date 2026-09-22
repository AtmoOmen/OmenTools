using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToNewGridania : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 格里达尼亚新街";

    public override uint SourceZone => 144;

    public override IReadOnlyList<uint> TargetZones => [132];

    public override Vector3 EventPosition => new(-38.2f, 0.0f, 100.5f);

    protected override uint EventID => 131184;

    public override uint Cost => 120;
}

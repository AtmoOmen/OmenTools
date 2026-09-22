using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ChurningMistsToDravanianForelands : EventSimpleYesTransportBase
{
    public override string DisplayName => "翻云雾海 → 龙堡参天高地";

    public override uint SourceZone => 400;

    public override IReadOnlyList<uint> TargetZones => [398];

    public override Vector3 EventPosition => new(198.0f, -68.6f, 706.6f);

    protected override uint EventID => 131200;
}

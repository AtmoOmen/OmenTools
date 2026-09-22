using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class EasternLaNosceaToLowerDecks : EventSimpleYesTransportBase
{
    public override string DisplayName => "东拉诺西亚 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone => 137;

    public override IReadOnlyList<uint> TargetZones => [129];

    public override Vector3 EventPosition => new(612.3f, 11.6f, 394.3f);

    protected override uint EventID => 131112;

    public override uint Cost => 40;
}

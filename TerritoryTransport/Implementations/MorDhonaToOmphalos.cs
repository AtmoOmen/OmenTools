using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class MorDhonaToOmphalos : EventSimpleYesTransportBase
{
    public override string DisplayName => "摩杜纳 → 翁法洛斯";

    public override uint SourceZone => 156;

    public override IReadOnlyList<uint> TargetZones => [1061];

    public override Vector3 EventPosition => new(155.4f, -26.9f, -440.6f);

    protected override uint EventID => 131472;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class AzysLlaToPillars : EventSimpleYesTransportBase
{
    public override string DisplayName => "魔大陆阿济兹拉 → 伊修加德砥柱层";

    public override uint SourceZone => 402;

    public override Vector3 EventPosition => new(-876.0f, -184.3f, -668.1f);

    protected override uint EventID => 131203;
}

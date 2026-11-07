using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class PillarsToAzysLla : EventSimpleYesTransportBase
{
    public override string DisplayName => "伊修加德砥柱层 → 魔大陆阿济兹拉";

    public override uint SourceZone => 419;

    public override Vector3 EventPosition => new(165.7f, -14.3f, 47.8f);

    protected override uint EventID => 131190;
}

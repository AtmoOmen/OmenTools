using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class GoldSaucerToKeyboundSquare : EventSimpleYesTransportBase
{
    public override string DisplayName => "金碟游乐场 → 键斗广场";

    public override uint SourceZone => 144;

    public override Vector3 EventPosition => new(4.6f, 4.0f, 62.8f);

    protected override uint EventID => 131633;
}

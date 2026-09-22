using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class LavenderBedsToCentralShroud : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "薰衣草苗圃 → 黑衣森林中央林区";

    public override uint SourceZone =>
        340;

    public override IReadOnlyList<uint> TargetZones =>
        [148];

    public override Vector3 EventPosition =>
        new(9.7f, 2.6f, 204.7f);

    protected override uint EventID =>
        131151;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

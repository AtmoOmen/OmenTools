using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class LavenderBedsToOldGridania : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "薰衣草苗圃 → 格里达尼亚旧街";

    public override uint SourceZone =>
        340;

    public override IReadOnlyList<uint> TargetZones =>
        [133];

    public override Vector3 EventPosition =>
        new(9.7f, 2.6f, 204.7f);

    protected override uint EventID =>
        131159;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

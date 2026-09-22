using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class ShiroganeToKugane : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "白银乡 → 黄金港";

    public override uint SourceZone =>
        641;

    public override IReadOnlyList<uint> TargetZones =>
        [628];

    public override Vector3 EventPosition =>
        new(-857.0f, 2.0f, -822.5f);

    protected override uint EventID =>
        131249;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

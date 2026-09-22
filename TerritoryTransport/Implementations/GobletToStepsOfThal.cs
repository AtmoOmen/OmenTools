using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class GobletToStepsOfThal : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "高脚孤丘 → 乌尔达哈来生回廊";

    public override uint SourceZone =>
        341;

    public override IReadOnlyList<uint> TargetZones =>
        [131];

    public override Vector3 EventPosition =>
        new(30.8f, -11.1f, -214.3f);

    protected override uint EventID =>
        131163;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class EmpyreumToFoundation : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "穹顶皓天 → 伊修加德基础层";

    public override uint SourceZone =>
        979;

    public override IReadOnlyList<uint> TargetZones =>
        [418];

    public override Vector3 EventPosition =>
        new(-882.0f, -15.2f, -693.5f);

    protected override uint EventID =>
        131396;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

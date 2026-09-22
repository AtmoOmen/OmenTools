using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.TerritoryTransport;

public sealed class MistToLowerDecks : EventSimpleStringTransportBase
{
    public override string DisplayName =>
        "海雾村 → 利姆萨·罗敏萨下层甲板";

    public override uint SourceZone =>
        339;

    public override IReadOnlyList<uint> TargetZones =>
        [129];

    public override Vector3 EventPosition =>
        new(-65.6f, 2.0f, 97.5f);

    protected override uint EventID =>
        131161;

    protected override string EventString => 
        LuminaWrapper.GetAddonText(6403);
}

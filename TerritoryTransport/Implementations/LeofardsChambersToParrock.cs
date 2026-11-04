using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LeofardsChambersToParrock : EventSimpleYesTransportBase
{
    public override string DisplayName => "利奥法德的房间 → 帕洛克系留基地";

    public override uint SourceZone => 568;

    public override IReadOnlyList<uint> TargetZones => [567];

    public override Vector3 EventPosition => new(0.0f, 1.0f, 9.4f);

    protected override uint EventID => 131226;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class AmhAraengToCrystarium : EventSimpleYesTransportBase
{
    public override string DisplayName => "安穆·艾兰 → 水晶都";

    public override uint SourceZone => 815;

    public override IReadOnlyList<uint> TargetZones => [819];

    public override Vector3 EventPosition => new(693.5f, -38.9f, -660.1f);

    protected override uint EventID => 131336;
}

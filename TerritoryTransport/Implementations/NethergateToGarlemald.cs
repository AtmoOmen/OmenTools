using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class NethergateToGarlemald : EventSimpleYesTransportBase
{
    public override string DisplayName => "神门之间 → 加雷马";

    public override uint SourceZone => 1024;

    public override IReadOnlyList<uint> TargetZones => [958];

    public override Vector3 EventPosition => new(0.2f, 0.0f, 49.1f);

    protected override uint EventID => 131449;
}

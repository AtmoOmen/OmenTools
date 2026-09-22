using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LabyrinthosToUltimaThule : EventSimpleYesTransportBase
{
    public override string DisplayName => "迷津 → 天外天垓";

    public override uint SourceZone => 956;

    public override IReadOnlyList<uint> TargetZones => [960];

    public override Vector3 EventPosition => new(-335.8f, -224.2f, 309.6f);

    protected override uint EventID => 131466;
}

using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class LabyrinthosToOldSharlayan : EventSimpleYesTransportBase
{
    public override string DisplayName => "迷津 → 旧萨雷安";

    public override uint SourceZone => 956;

    public override Vector3 EventPosition => new(-26.7f, 179.7f, -823.5f);

    protected override uint EventID => 131408;
}

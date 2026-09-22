using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class YakTelToTuliyollal : EventSimpleYesTransportBase
{
    public override string DisplayName => "亚克特尔树海 → 图莱尤拉";

    public override uint SourceZone => 1189;

    public override IReadOnlyList<uint> TargetZones => [1185];

    public override Vector3 EventPosition => new(24.4f, 8.2f, -662.2f);

    protected override uint EventID => 131552;
}

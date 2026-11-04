using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ExcavationTunnelsToCopiedFactory : EventSimpleYesTransportBase
{
    public override string DisplayName => "机械遗迹坑道 → 复制工厂废墟";

    public override uint SourceZone => 895;

    public override Vector3 EventPosition => new(-0.1f, 0.0f, -93.3f);

    protected override uint EventID => 131350;
}

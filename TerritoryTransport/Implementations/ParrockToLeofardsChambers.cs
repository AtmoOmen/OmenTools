using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;

namespace OmenTools.TerritoryTransport;

public sealed class ParrockToLeofardsChambers : EventSimpleYesTransportBase
{
    public override string DisplayName => "帕洛克系留基地 → 利奥法德的房间";

    public override uint SourceZone => 567;

    public override Vector3 EventPosition => new(22.3f, 3.6f, -18.8f);

    protected override uint EventID => 131225;
}

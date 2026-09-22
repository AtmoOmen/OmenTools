using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class PhaennaToMareLamentorum : EventTransportBase
{
    public override string DisplayName =>
        "法恩娜 → 叹息海";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        [TARGET_ZONE];

    public override Vector3 EventPosition =>
        NPCPosition;

    protected override uint EventID =>
        NPC_EVENT_ID;

    protected override void EnqueueEventTrigger(TaskHelper taskHelper, uint targetZone, int weight)
    {
        taskHelper.Enqueue
        (
            () => new EventStartPackt(LocalPlayerState.EntityID, NPC_EVENT_ID).Send(),
            "发送交互包",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () =>
            {
                AddonTalkEvent.ClickNext();
                return AddonSelectStringEvent.Select(1);
            },
            "进入叹息海",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE  = 1291;
    private const uint TARGET_ZONE  = 959;
    private const uint NPC_EVENT_ID = 721817;

    private static readonly Vector3 NPCPosition = new(279.1f, 52.0f, -378.6f);

    #endregion
}

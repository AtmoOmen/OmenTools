using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class FirmamentToFoundation : EventTransportBase
{
    public override string DisplayName =>
        "天穹街 → 伊修加德基础层";

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
            () =>
            {
                new EventStartPackt(LocalPlayerState.EntityID, NPC_EVENT_ID).Send();
                new EventCompletePackt(NPC_EVENT_ID, EVENT_CATEGORY, EVENT_PARAM).Send();
            },
            "发送交互包",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () =>
            {
                AddonTalkEvent.ClickNext();
                return AddonSelectYesnoEvent.ClickYes();
            },
            "点击确认",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE    = 886;
    private const uint TARGET_ZONE    = 418;
    private const uint NPC_EVENT_ID   = 131343;
    private const uint EVENT_CATEGORY = 16777216;
    private const uint EVENT_PARAM    = 1;

    private static readonly Vector3 NPCPosition = new(13.4f, -15.2f, 180.5f);

    #endregion
}

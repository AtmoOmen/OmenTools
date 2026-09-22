using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class FoundationToFirmament : EventTransportBase
{
    public override string DisplayName =>
        "伊修加德基础层 → 天穹街";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        [TARGET_ZONE];

    public override Vector3 EventPosition =>
        AetherytePosition;

    protected override uint EventID =>
        NPC_EVENT_ID;

    protected override void EnqueueEventTrigger(TaskHelper taskHelper, uint targetZone, int weight) =>
        taskHelper.Enqueue
        (
            () =>
            {
                new EventStartPackt(LocalPlayerState.EntityID, NPC_EVENT_ID).Send();
                new EventCompletePackt(NPC_EVENT_ID, EVENT_CATEGORY, EVENT_PARAM).Send();
            },
            "进入目标区域",
            weight: weight
        );

    #region 常量

    private const uint SOURCE_ZONE    = 418;
    private const uint TARGET_ZONE    = 886;
    private const uint NPC_EVENT_ID   = 327750;
    private const uint EVENT_CATEGORY = 16777216;
    private const uint EVENT_PARAM    = 10;

    private static readonly Vector3 AetherytePosition = new(-71f, 8f, 38f);

    #endregion
}

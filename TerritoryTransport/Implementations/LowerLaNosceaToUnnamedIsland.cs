using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class LowerLaNosceaToUnnamedIsland : EventTransportBase
{
    public override string DisplayName =>
        "拉诺西亚低地 → 无名岛";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        [TARGET_ZONE];

    public override Vector3 EventPosition =>
        NPCPosition;

    protected override uint EventID => NPC_EVENT_ID;

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
                return AddonSelectStringEvent.Select(0);
            },
            "选择进入无人岛",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () =>
            {
                AddonTalkEvent.ClickNext();
                return AddonSelectYesnoEvent.ClickYes();
            },
            "点击确认进入无人岛",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE  = 135;
    private const uint TARGET_ZONE  = 1055;
    private const uint NPC_EVENT_ID = 721694;

    private static readonly Vector3 NPCPosition = new(172, 12, 642);

    #endregion
}

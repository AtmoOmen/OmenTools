using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class FirmamentToDiadem : EventTransportBase
{
    public override string DisplayName =>
        "天穹街 → 天上福地云冠群岛";

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
                return AddonSelectStringEvent.Select(DutyName);
            },
            "选择副本",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () =>
            {
                AddonTalkEvent.ClickNext();
                return AddonSelectYesnoEvent.ClickYes();
            },
            "点击确认进入副本",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE        = 886;
    private const uint TARGET_ZONE        = 939;
    private const uint NPC_EVENT_ID       = 721532;
    private const uint DIAMDEM_CONTENT_ID = 753;

    private static readonly Vector3 NPCPosition = new(-19.6f, -16f, 143f);

    private static readonly string DutyName = LuminaWrapper.GetContentName(DIAMDEM_CONTENT_ID);

    #endregion
}

using System.Collections.Frozen;
using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class KuganeToUreka : EventTransportBase
{
    public override string DisplayName =>
        "黄金港 → 禁地优雷卡";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        ZoneToContent.Keys;

    public override Vector3 EventPosition =>
        NPCPosition;

    protected override uint EventID =>
        NPC_EVENT_ID;

    protected override void EnqueueEventTrigger(TaskHelper taskHelper, uint targetZone, int weight)
    {
        if (!ZoneToContent.TryGetValue(targetZone, out var contentID)) return;

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
                return AddonSelectStringEvent.Select(LuminaWrapper.GetContentName(contentID));
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
            "确认进入副本",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE  = 628;
    private const uint NPC_EVENT_ID = 721355;

    private static readonly FrozenDictionary<uint, uint> ZoneToContent = new Dictionary<uint, uint>
    {
        // 常风之地
        [732] = 283,
        // 恒冰之地
        [763] = 581,
        // 涌火之地
        [795] = 598,
        // 丰水之地
        [827] = 639
    }.ToFrozenDictionary();

    private static readonly Vector3 NPCPosition = new(-114.3f, -5f, 150f);

    #endregion
}

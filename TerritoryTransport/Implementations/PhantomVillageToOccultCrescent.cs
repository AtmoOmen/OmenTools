using System.Collections.Frozen;
using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed class PhantomVillageToOccultCrescent : EventTransportBase
{
    public override string DisplayName =>
        "幻境村 → 蜃景幻界新月岛";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        ZoneToContent.Keys;

    public override Vector3 EventPosition => NPCPosition;

    protected override uint EventID => NPC_EVENT_ID;

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
                return AddonSelectStringEvent.Select(0);
            },
            "确认进入副本",
            weight: weight
        );
    }

    #region 常量

    private const uint SOURCE_ZONE  = 1278;
    private const uint NPC_EVENT_ID = 721825;

    private static readonly FrozenDictionary<uint, uint> ZoneToContent = new Dictionary<uint, uint>
    {
        // 南征之章
        [1252] = 1018,
        // 北征之章
        [1346] = 1093
    }.ToFrozenDictionary();

    private static readonly Vector3 NPCPosition = new(-71.93f, 5f, -16.02f);

    #endregion
}

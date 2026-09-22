using System.Collections.Frozen;
using System.Numerics;
using OmenTools.TerritoryTransport.Abstractions;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport;

public sealed unsafe class MareLamentorumToCosmicExploration : EventTransportBase
{
    public override string DisplayName =>
        "叹息海 → 宇宙探索";

    public override uint SourceZone =>
        SOURCE_ZONE;

    public override IReadOnlyList<uint> TargetZones =>
        [.. Zones];

    public override Vector3 EventPosition =>
        NPCPosition;

    protected override uint EventID =>
        NPC_EVENT_ID;

    private uint redirectTargetZone;

    public MareLamentorumToCosmicExploration() =>
        GamePacketManager.Instance().RegPreSendPacket(OnPreSendPacket);

    public override void Cleanup() =>
        GamePacketManager.Instance().Unreg(OnPreSendPacket);

    protected override void EnqueueEventTrigger(TaskHelper taskHelper, uint targetZone, int weight)
    {
        taskHelper.Enqueue
        (
            () => redirectTargetZone = targetZone,
            "赋值目标区域",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () => new EventStartPackt(LocalPlayerState.EntityID, NPC_EVENT_ID).Send(),
            "发送交互包",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () => AddonSelectStringEvent.Select(3),
            "进入宇宙探索区域",
            weight: weight
        );
    }

    private void OnPreSendPacket(ref bool isPrevented, int opcode, ref nint packet, ref bool isPrioritize)
    {
        if (opcode != UpstreamOpcode.EventCompleteOpcode) return;
        if (GameState.TerritoryType != SOURCE_ZONE || redirectTargetZone == 0) return;

        var data = (EventCompletePackt*)packet;
        if (data->EventID != NPC_EVENT_ID) return;

        if (data->Category == CATEGORY_TARGET_SELECT)
            data->Param1 = redirectTargetZone;

        if (data->Category == CATEGORY_TARGET_CONFIRM)
        {
            data->Param0       = redirectTargetZone;
            redirectTargetZone = 0;
        }
    }

    #region 常量

    private const uint SOURCE_ZONE             = 959;
    private const uint NPC_EVENT_ID            = 327855;
    private const uint CATEGORY_TARGET_SELECT  = 0x2000000;
    private const uint CATEGORY_TARGET_CONFIRM = 0x1000064;

    private static readonly Vector3 NPCPosition = new(-5.3f, -131.1f, -504.0f);

    private static readonly FrozenSet<uint> Zones =
    [
        1237, // 憧憬湾
        1291, // 法恩娜
        1310, // 俄匊斯
        1319  // 奥克塞西亚
    ];

    #endregion
}

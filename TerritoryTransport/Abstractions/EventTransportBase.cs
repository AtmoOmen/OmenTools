using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Threading.TaskHelper;
using WarpConditionFailure = FFXIVClientStructs.Expansion.FFXIV.Client.Game.Event.WarpEventHandler.WarpConditionFailure;
using WarpEventHandler = FFXIVClientStructs.Expansion.FFXIV.Client.Game.Event.WarpEventHandler;

namespace OmenTools.TerritoryTransport.Abstractions;

public abstract unsafe class EventTransportBase
{
    public abstract uint SourceZone { get; }

    public virtual IReadOnlyList<uint> TargetZones => field ??= [LuminaGetter.Get<Warp>().GetRow(EventID).TerritoryType.RowId];

    public abstract string DisplayName { get; }

    public abstract    Vector3 EventPosition { get; }
    protected abstract uint    EventID       { get; }

    public virtual uint Cost => WarpData is { } warp ?
                                    warp.WarpCondition.Value.Gil :
                                    0U;

    private Warp? WarpData
    {
        get
        {
            if (EventID >> 16 != (uint)EventHandlerContent.Warp)
                return null;

            return field ??= LuminaGetter.Get<Warp>().GetRow(EventID);
        }
    }

    /// <summary>
    ///     判断线路是否满足当前进度、最高职业等级及解锁条件，不因暂时缺少金币而判为未解锁。
    /// </summary>
    /// <returns>已登录且线路的非金币条件全部满足时为真；不接受自定义 Warp 逻辑。</returns>
    public bool IsUnlocked() =>
        GameState.IsLoggedIn &&
        (WarpData is not { } warp ||
         (warp.WarpLogic.RowId                                                                         == NORMAL_WARP_LOGIC &&
          (GetConditionFailureFlags(warp.WarpCondition.Value) & ~WarpConditionFailure.InsufficientGil) == WarpConditionFailure.None));

    public bool IsAvailable() =>
        GameState.IsLoggedIn &&
        (WarpData is not { } warp ||
         (warp.WarpLogic.RowId                               == NORMAL_WARP_LOGIC &&
          GetConditionFailureFlags(warp.WarpCondition.Value) == WarpConditionFailure.None));

    public bool IsEventNearby() =>
        EventFramework.Instance()->IsEventIDNearby(EventID);

    public IEnumerable<uint> EnumerateReachableTargets
    (
        uint source
    ) =>
        source == SourceZone && IsAvailable() ?
            TargetZones :
            [];

    public void Enqueue
    (
        TaskHelper taskHelper,
        uint       targetZone,
        int        weight
    )
    {
        EnqueueDismount(taskHelper, weight);

        EnqueuePreEventAction(taskHelper, targetZone, weight);
        taskHelper.Enqueue
        (
            () =>
            {
                if (GameState.TerritoryType != SourceZone ||
                    !TargetZones.Contains(targetZone)     ||
                    !IsAvailable())
                    taskHelper.Abort();
            },
            "检查区域交通条件",
            weight: weight
        );
        EnqueueEventTrigger(taskHelper, targetZone, weight);
        taskHelper.Enqueue
        (
            WaitForZoneReady(targetZone),
            $"等待进入目标区域: {targetZone}",
            weight: weight
        );
        EnqueuePostEventAction(taskHelper, targetZone, weight);
    }

    public virtual void Cleanup()
    {
    }

    #region 工具

    protected static Func<bool> WaitForZoneReady
    (
        uint zone
    ) =>
        () => GameState.TerritoryType == zone &&
              LocalPlayerState.Object != null;

    #endregion

    private static WarpConditionFailure GetConditionFailureFlags(WarpCondition condition)
    {
        WarpEventHandler handler = default;
        handler.Gil            = condition.Gil;
        handler.ClassLevel     = condition.ClassLevel;
        handler.CompleteParam  = condition.CompleteParam;
        handler.RequiredQuest1 = (ushort)condition.RequiredQuest1.RowId;
        handler.RequiredQuest2 = (ushort)condition.RequiredQuest2.RowId;
        handler.RequiredQuest3 = (ushort)condition.RequiredQuest3.RowId;
        handler.RequiredQuest4 = (ushort)condition.RequiredQuest4.RowId;
        handler.QuestReward    = condition.QuestReward;
        return handler.GetConditionFailureFlags();
    }

    private static void EnqueueDismount
    (
        TaskHelper taskHelper,
        int        weight
    ) =>
        taskHelper.Enqueue
        (
            () =>
            {
                if (!ICondition.Instance()[ConditionFlag.Mounted])
                    return true;

                MovementManager.Instance().Dismount(false);
                return false;
            },
            "下坐骑",
            weight: weight
        );

    protected virtual void EnqueuePreEventAction
    (
        TaskHelper taskHelper,
        uint       target,
        int        weight
    )
    {
    }

    protected abstract void EnqueueEventTrigger
    (
        TaskHelper taskHelper,
        uint       targetZone,
        int        weight
    );

    protected virtual void EnqueuePostEventAction
    (
        TaskHelper taskHelper,
        uint       target,
        int        weight
    )
    {
    }

    #region 常量

    private const uint NORMAL_WARP_LOGIC = 1;

    #endregion
}

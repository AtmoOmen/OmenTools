using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport.Abstractions;

public abstract unsafe class EventTransportBase
{
    public abstract uint                SourceZone  { get; }
    public abstract IReadOnlyList<uint> TargetZones { get; }
    public abstract string              DisplayName { get; }

    public abstract    Vector3 EventPosition { get; }
    protected abstract uint    EventID       { get; }

    public virtual uint Cost => 0;

    public bool IsEventNearby() =>
        EventFramework.Instance()->IsEventIDNearby(EventID);

    public IEnumerable<uint> EnumerateReachableTargets
    (
        uint source
    ) =>
        source == SourceZone ?
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
}

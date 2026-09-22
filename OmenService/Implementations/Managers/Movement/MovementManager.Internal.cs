using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud;
using OmenTools.Info.Game.AetheryteRecord.Data;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Info.Lumina;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using OmenTools.Interop.Game.Models.Native;
using OmenTools.OmenService.Abstractions;
using OmenTools.TerritoryTransport.Abstractions;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;

namespace OmenTools.OmenService;

public partial class MovementManager
{
    private static readonly CompSig CurrentMoveModeInternalSig = new("0F B6 0D ?? ?? ?? ?? B8");
    private                 byte*   CurrentMoveModeInternal    = null!;

    private static readonly CompSig CurrentZoneMoveStateBaseInstanceSig =
        new
        (
            "48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 48 89 5C 24 ?? 57 48 83 EC ?? 48 8B B9"
        );
    private byte* CurrentZoneMoveStateBaseInstance = null!;

    private static readonly CompSig GetCurrentZoneMoveStateSig = new("E8 ?? ?? ?? ?? 66 83 C8 40");
    private delegate ushort GetCurrentZoneMoveStateDelegate
    (
        byte* instance
    );
    private GetCurrentZoneMoveStateDelegate GetCurrentZoneMoveState = null!;

    private static uint TicketUsageGilSetting =>
        IGameConfig.Instance().UiConfig.GetUInt("TelepoTicketGilSetting");

    private static uint TicketUsageType =>
        IGameConfig.Instance().UiConfig.GetUInt("TelepoTicketUseType");

    private Config config = null!;

    protected override unsafe void Init()
    {
        config = LoadConfig<Config>() ?? new();

        CurrentMoveModeInternal          = CurrentMoveModeInternalSig.GetStatic<byte>();
        CurrentZoneMoveStateBaseInstance = CurrentZoneMoveStateBaseInstanceSig.GetStatic<byte>();
        GetCurrentZoneMoveState          = GetCurrentZoneMoveStateSig.GetDelegate<GetCurrentZoneMoveStateDelegate>();

        Type[] types;

        try
        {
            types = typeof(EventTransportBase).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException)
        {
            return;
        }

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (!typeof(EventTransportBase).IsAssignableFrom(type)) continue;
            if (type.GetConstructor(Type.EmptyTypes) == null) continue;

            if (Activator.CreateInstance(type) is EventTransportBase method)
                EventTransportations.Add(method);
        }

        DLog.Debug($"[MovementManager] 已注册 {EventTransportations.Count} 个区域变更方式");
    }

    protected override void Uninit()
    {
        foreach (var method in EventTransportations)
            method.Cleanup();
        EventTransportations.Clear();

        TaskHelper    = null;
        IsBusyHandler = null;
    }

    #region 平滑移动

    private void ExecuteTPSmoothPosition
    (
        IPlayerCharacter localPlayer,
        Vector3          pos
    )
    {
        var isFlight = ICondition.Instance()[ConditionFlag.Mounted] && Control.CanFly;

        if (isFlight)
        {
            unsafe
            {
                PlayerController.Instance()->MoveControllerFly.MountPosition = pos;
            }
        }
        else
        {
            unsafe
            {
                localPlayer.ToStruct()->SetPosition(pos.X, pos.Y, pos.Z);
            }
        }

        if (ICondition.Instance().IsBoundByDuty)
        {
            var moveType = GetInstanceMoveType
            (
                isFlight ?
                    PositionUpdateInstancePacket.MoveType.Fly0 :
                    PositionUpdateInstancePacket.MoveType.NormalMove0
            );
            new PositionUpdateInstancePacket(localPlayer.Rotation, pos, moveType).Send();
        }
        else
        {
            var moveType = GetMoveType
            (
                isFlight ?
                    PositionUpdatePacket.MoveType.Fly0 :
                    PositionUpdatePacket.MoveType.NormalMove0
            );
            new PositionUpdatePacket(localPlayer.Rotation, pos, moveType).Send();
        }
    }

    private static Vector3 GetTPSmoothNextPosition
    (
        IPlayerCharacter localPlayer,
        Vector3          targetPosition,
        float            speed,
        out bool         isArrived
    )
    {
        var currentPosition = localPlayer.Position;
        var targetXZ        = new Vector3(targetPosition.X, currentPosition.Y, targetPosition.Z);
        var deltaXZ         = targetXZ - currentPosition;
        var distance        = deltaXZ.Length();

        isArrived = distance < 0.1f;
        if (isArrived || speed * GameState.DeltaTime >= distance)
            return targetPosition;

        var nextPosition = currentPosition + (deltaXZ / distance * speed * GameState.DeltaTime);
        nextPosition.Y = targetPosition.Y;
        return nextPosition;
    }

    #endregion

    #region 路径

    public static List<ZoneHop> FindBestPath
    (
        uint source,
        uint target
    )
    {
        if (source == target) return [];

        var stateKey  = new PathState(source, Vector3.Zero);
        var bestKnown = new Dictionary<PathState, PathCost>();
        var cameFrom  = new Dictionary<PathState, PathEdge>();
        var pq        = new PriorityQueue<PathState, PathCost>();

        bestKnown[stateKey] = new PathCost(0, 0);
        pq.Enqueue(stateKey, new PathCost(0, 0));

        PathState? targetState = null;

        while (pq.TryDequeue(out var state, out var priority))
        {
            if (!bestKnown.TryGetValue(state, out var known)  ||
                known.EffectiveCost != priority.EffectiveCost ||
                known.Hops          != priority.Hops)
                continue;

            if (state.Zone == target)
            {
                targetState = state;
                break;
            }

            foreach (var hop in EnumerateEdges(state.Zone, state.Zone == source))
            {
                var arrivalPos = hop switch
                {
                    AetheryteHop a   => a.Record.Position,
                    TelepotTownHop t => t.Target.Position,
                    _                => Vector3.Zero
                };

                var nextState = new PathState(hop.TargetZone, arrivalPos);

                var hopCumulativePenalty = !Instance().AlwaysMinGilTransport && priority.Hops >= 1 ?
                                               (uint)priority.Hops * Instance().config.HopCumulativePenalty :
                                               0U;
                var hopPenalty = hopCumulativePenalty + 0U;

                var hopCost = hop switch
                {
                    AetheryteHop a => TeleportCostCalculator.GetTeleportCost
                    (
                        a.Record.GetData(),
                        state.Zone,
                        a.Record.RowID,
                        a.Record.Group,
                        a.Record.IsAetheryte,
                        a.Record.IsHouse
                    ),
                    EventHop e => e.Method.Cost,
                    _          => 0U
                };

                var hopIncrement = hopCost                + hopPenalty;
                var newHops      = priority.Hops          + 1;
                var newEffective = priority.EffectiveCost + hopIncrement;

                if (bestKnown.TryGetValue(nextState, out var nextKnown) &&
                    (nextKnown.EffectiveCost < newEffective ||
                     (nextKnown.EffectiveCost == newEffective && nextKnown.Hops <= newHops)))
                    continue;

                bestKnown[nextState] = new PathCost(newEffective, newHops);
                cameFrom[nextState]  = new PathEdge(hop, state, hopIncrement);
                pq.Enqueue(nextState, new PathCost(newEffective, newHops));
            }
        }

        if (targetState is not { } ts || !cameFrom.ContainsKey(ts))
            return [];

        // 总是使用或不低于金额总是使用
        if (TicketUsageType is 1 or 4                                  &&
            LocalPlayerState.GetItemCount(TELEPORT_TICKET_ITEM_ID) > 0 &&
            AetheryteRecordManager.Instance().GetNearestAetheryte(target, Vector3.Zero) is { } aetheryte)
        {
            var directCost = TeleportCostCalculator.GetTeleportCost
            (
                aetheryte.GetData(),
                source,
                aetheryte.RowID,
                aetheryte.Group,
                aetheryte.IsAetheryte,
                aetheryte.IsHouse
            );

            if (TicketUsageType == 1 || directCost > TicketUsageGilSetting)
            {
                var hasAetheryte = false;
                var checkKey     = ts;

                while (checkKey.Zone != source || checkKey.Pos != Vector3.Zero)
                {
                    var (ckHop, ckPrev, _) = cameFrom[checkKey];

                    if (ckHop is AetheryteHop)
                    {
                        hasAetheryte = true;
                        break;
                    }

                    checkKey = ckPrev;
                }

                if (hasAetheryte)
                    return [new AetheryteHop(aetheryte)];
            }
        }

        var path = new Stack<ZoneHop>();
        var cur  = ts;

        while (cur.Zone != source || cur.Pos != Vector3.Zero)
        {
            var (hop, prev, _) = cameFrom[cur];
            path.Push(hop);
            cur = prev;
        }

        return [.. path];
    }

    private static unsafe List<ZoneHop> EnumerateEdges
    (
        uint zone,
        bool isRoot
    )
    {
        var edges = new List<ZoneHop>();

        foreach (var er in LGBAssets.ZoneLineExitRanges.GetValueOrDefault(zone, []))
            edges.Add(new ExitRangeHop(er.TargetZone, er.Position));

        var zoneAetheryte =
            AetheryteRecordManager.Instance().GetNearestAetheryte(zone, Vector3.Zero);

        if (zoneAetheryte is { Group: > 0, ZoneID: var aetheryteZoneID }                                 &&
            LuminaGetter.GetRowOrDefault<TerritoryType>(aetheryteZoneID).TerritoryIntendedUse.RowId == 0 &&
            (!isRoot ||
             EventFramework.Instance()->TryGetNearestEventID
             (
                 x => x.EventId.ContentId is EventHandlerContent.Aetheryte,
                 _ => true,
                 Vector3.Zero,
                 out _
             )))
        {
            foreach (var record in AetheryteRecordManager.Instance().AllRecords)
            {
                if (record.ZoneID == zone) continue;

                if (AetheryteRecordManager.Instance().AllRecords.FirstOrDefault
                    (x => x.Group  == zoneAetheryte.Group &&
                          x.ZoneID == record.ZoneID
                    )
                    is null)
                    continue;

                edges.Add(new TelepotTownHop(record));
            }
        }

        foreach (var method in Instance().EventTransportations)
        foreach (var next in method.EnumerateReachableTargets(zone))
        {
            if (GameState.TerritoryType == method.SourceZone &&
                !method.IsEventNearby()                      &&
                Sheets.SpeedDetectionZones.ContainsKey(method.SourceZone))
                continue;
            edges.Add(new EventHop(method, next));
        }

        foreach (var record in AetheryteRecordManager.Instance().AllRecords)
        {
            if (record.ZoneID == zone) continue;
            if (!AetheryteRecords.AethernetGroups.Contains(record.Group)) continue;
            edges.Add(new AetheryteHop(record));
        }

        return edges;
    }

    #endregion

    public class Config : OmenServiceConfig
    {
        public bool AlwaysMinGilTransport { get; set; }

        public uint HopCumulativePenalty { get; set; } = 5000;

        public void Save() =>
            this.Save(Instance());
    }

    private readonly record struct PathState
    (
        uint    Zone,
        Vector3 Pos
    );

    private readonly record struct PathCost
    (
        uint EffectiveCost,
        int  Hops
    ) : IComparable<PathCost>
    {
        public int CompareTo
        (
            PathCost other
        )
        {
            var cmp = EffectiveCost.CompareTo(other.EffectiveCost);
            return cmp != 0 ?
                       cmp :
                       Hops.CompareTo(other.Hops);
        }
    }

    private readonly record struct PathEdge
    (
        ZoneHop   Hop,
        PathState PrevState,
        uint      HopTotalCost
    );

    #region 常量

    private const uint TELEPORT_TICKET_ITEM_ID = 7569;

    #endregion
}

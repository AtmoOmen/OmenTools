using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud;
using OmenTools.Info.Game.AetheryteRecord;
using OmenTools.Info.Game.AetheryteRecord.Data;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Info.Lumina;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using OmenTools.Interop.Game.Models;
using PlayerController = FFXIVClientStructs.Expansion.FFXIV.Client.Game.Control.PlayerController;
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

    /// <summary>
    ///     按照金币费用和换乘惩罚选择到达目的地区域的路线，并应用当前传送券策略。
    /// </summary>
    /// <param name="source">出发区域编号。</param>
    /// <param name="target">目的地区域编号。</param>
    /// <returns>按执行顺序排列的交通分段；无需移动或无法到达时为空。</returns>
    public static List<ZoneHop> FindBestPath(uint source, uint target)
    {
        if (source == target)
            return [];

        var context = CapturePathSearch(source);
        var routes  = SearchPaths(context, source, [target], CancellationToken.None);
        if (!routes.TryGetValue(target, out var node))
            return [];

        var path = new Stack<ZoneHop>();

        while (node.Previous is { } previous)
        {
            path.Push(node.Hop);
            node = previous;
        }

        return [.. path];
    }

    /// <summary>
    ///     从同一出发区域搜索多个目的地区域，返回所选路线的实际金币费用，不包含换乘惩罚。
    /// </summary>
    /// <param name="source">出发区域编号。</param>
    /// <param name="targets">目的地区域编号集合。</param>
    /// <returns>可到达的目的地区域及其金币费用；当前区域的费用为零。</returns>
    public static Dictionary<uint, uint> FindRouteCosts(uint source, IEnumerable<uint> targets)
    {
        var context = CapturePathSearch(source);
        return SearchPaths(context, source, [.. targets], CancellationToken.None)
            .ToDictionary(x => x.Key, x => x.Value.GilCost);
    }

    internal static async Task<Dictionary<uint, uint>> FindRouteCostsAsync
    (
        uint                source,
        IReadOnlyList<uint> targets,
        CancellationToken   cancellationToken
    )
    {
        var context = await IFramework.Instance().Run
                      (
                          () => CapturePathSearch(source),
                          cancellationToken
                      ).ConfigureAwait(false);

        return await Task.Run
               (
                   () => SearchPaths(context, source, [.. targets], cancellationToken)
                       .ToDictionary(x => x.Key, x => x.Value.GilCost),
                   cancellationToken
               ).ConfigureAwait(false);
    }

    private static unsafe PathSearchContext CapturePathSearch(uint source)
    {
        var manager = Instance();
        var records = AetheryteRecordManager.Instance().AllRecords
                                            .Where(x => AetheryteRecords.AethernetGroups.Contains(x.Group))
                                            .ToArray();
        var nearest = records.Where(x => x.IsUnlocked())
                             .GroupBy(x => x.ZoneID)
                             .ToDictionary(x => x.Key, x => x.OrderBy(record => record.Position.LengthSquared()).First());
        var groups = records.GroupBy(x => x.Group).ToDictionary(x => x.Key, x => x.ToArray());
        var edges  = new Dictionary<uint, List<ZoneHop>>();

        foreach (var (zone, exits) in LGBAssets.ZoneLineExitRanges)
            edges[zone] = [.. exits.Select(x => new ExitRangeHop(x.TargetZone, x.Position))];

        foreach (var (zone, aetheryte) in nearest)
        {
            if (aetheryte.Group                                                              == 0 ||
                LuminaGetter.GetRowOrDefault<TerritoryType>(zone).TerritoryIntendedUse.RowId != 0)
                continue;

            if (zone == source &&
                !EventFramework.Instance()->TryGetNearestEventID
                (
                    x => x.EventId.ContentId is EventHandlerContent.Aetheryte,
                    _ => true,
                    Vector3.Zero,
                    out _
                ))
                continue;

            if (!edges.TryGetValue(zone, out var zoneEdges))
                edges[zone] = zoneEdges = [];

            foreach (var record in groups[aetheryte.Group])
            {
                if (record.ZoneID != zone)
                    zoneEdges.Add(new TelepotTownHop(record));
            }
        }

        foreach (var method in manager.EventTransportations)
        {
            if (GameState.TerritoryType == method.SourceZone              &&
                Sheets.SpeedDetectionZones.ContainsKey(method.SourceZone) &&
                !method.IsEventNearby())
                continue;

            if (!edges.TryGetValue(method.SourceZone, out var zoneEdges))
                edges[method.SourceZone] = zoneEdges = [];

            foreach (var target in method.EnumerateReachableTargets(method.SourceZone))
                zoneEdges.Add(new EventHop(method, target));
        }

        var ticketType = TicketUsageType;
        var useTickets = ticketType is 1 or 4 &&
                         LocalPlayerState.GetItemCount(TELEPORT_TICKET_ITEM_ID) > 0;

        return new
        (
            edges.ToDictionary
            (
                x => x.Key,
                x => x.Value.Select
                (hop => new PathConnection
                 (
                     hop,
                     hop is EventHop transport ?
                         transport.Method.Cost :
                         0
                 )
                ).ToArray()
            ),
            [.. records.Select(x => new AetheryteHop(x))],
            nearest,
            TeleportCostCalculator.CaptureTeleportCosts(records),
            manager.AlwaysMinGilTransport ?
                0 :
                manager.config.HopCumulativePenalty,
            useTickets,
            ticketType,
            TicketUsageGilSetting
        );
    }

    private static Dictionary<uint, PathNode> SearchPaths
    (
        PathSearchContext context,
        uint              source,
        HashSet<uint>     pendingTargets,
        CancellationToken cancellationToken
    )
    {
        var root      = new PathNode(new(source, Vector3.Zero), new(0, 0), 0, false, null, null);
        var bestKnown = new Dictionary<PathState, PathNode> { [root.State] = root };
        var routes    = new Dictionary<uint, PathNode>();
        var edges     = new Dictionary<uint, PathConnection[]>();
        var queue     = new PriorityQueue<PathNode, PathCost>();
        queue.Enqueue(root, root.Cost);

        while (pendingTargets.Count > 0 &&
               queue.TryDequeue(out var node, out _))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ReferenceEquals(bestKnown[node.State], node))
                continue;

            if (pendingTargets.Remove(node.State.Zone))
            {
                var result = node;

                if (context.UseTickets &&
                    node.HasAetheryte  &&
                    context.NearestAetherytes.TryGetValue(node.State.Zone, out var aetheryte))
                {
                    var directCost = context.TeleportCost(source, aetheryte);

                    if (context.TicketType == 1 ||
                        directCost         > context.TicketThreshold)
                        result = new(node.State, node.Cost, directCost, true, root, new AetheryteHop(aetheryte));
                }

                routes[node.State.Zone] = result;
            }

            if (pendingTargets.Count == 0)
                break;

            if (!edges.TryGetValue(node.State.Zone, out var zoneEdges))
            {
                var connections = new List<PathConnection>(context.Aetherytes.Length);

                if (context.Edges.TryGetValue(node.State.Zone, out var localEdges))
                    connections.AddRange(localEdges);

                foreach (var aetheryte in context.Aetherytes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (aetheryte.TargetZone != node.State.Zone)
                        connections.Add(new(aetheryte, context.TeleportCost(node.State.Zone, aetheryte.Record)));
                }

                edges[node.State.Zone] = zoneEdges = [.. connections];
            }

            foreach (var (hop, connectionCost) in zoneEdges)
            {
                var arrival = hop switch
                {
                    AetheryteHop aetheryte => aetheryte.Record.Position,
                    TelepotTownHop town    => town.Target.Position,
                    _                      => Vector3.Zero
                };
                var state = new PathState(hop.TargetZone, arrival);
                var cost = new PathCost
                (
                    node.Cost.EffectiveCost + connectionCost + ((uint)node.Cost.Hops * context.HopPenalty),
                    node.Cost.Hops          + 1
                );

                if (bestKnown.TryGetValue(state, out var known) &&
                    known.Cost.CompareTo(cost) <= 0)
                    continue;

                var next = new PathNode
                (
                    state,
                    cost,
                    node.GilCost + connectionCost,
                    node.HasAetheryte || hop is AetheryteHop,
                    node,
                    hop
                );
                bestKnown[state] = next;
                queue.Enqueue(next, cost);
            }
        }

        return routes;
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

    private sealed record PathSearchContext
    (
        Dictionary<uint, PathConnection[]> Edges,
        AetheryteHop[]                     Aetherytes,
        Dictionary<uint, AetheryteRecord>  NearestAetherytes,
        Func<uint, AetheryteRecord, uint>  TeleportCost,
        uint                               HopPenalty,
        bool                               UseTickets,
        uint                               TicketType,
        uint                               TicketThreshold
    );

    private sealed record PathNode
    (
        PathState State,
        PathCost  Cost,
        uint      GilCost,
        bool      HasAetheryte,
        PathNode? Previous,
        ZoneHop?  Hop
    );

    private readonly record struct PathConnection
    (
        ZoneHop Hop,
        uint    Cost
    );

    #region 常量

    private const uint TELEPORT_TICKET_ITEM_ID = 7569;

    #endregion
}

using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.AetheryteRecord;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     传送费用本地计算器
/// </summary>
public static class TeleportCostCalculator
{
    /// <summary>
    ///     按移动管理器选出的实际路线，累加各段以太传送费用及区域交通费用，不计入选路惩罚。
    /// </summary>
    /// <param name="sourceZoneID">出发区域的 TerritoryType 行编号。</param>
    /// <param name="targetZoneID">目的地区域的 TerritoryType 行编号。</param>
    /// <returns>所选路线的金币总费用；当前已在目的地区域时为零，无可用路线时为无符号整数最大值。</returns>
    public static uint GetRouteCost(uint sourceZoneID, uint targetZoneID) =>
        GetRouteCosts(sourceZoneID, [targetZoneID]).GetValueOrDefault(targetZoneID, uint.MaxValue);

    /// <summary>
    ///     从同一出发区域批量计算多个目的地区域的路线费用。
    /// </summary>
    /// <param name="sourceZoneID">出发区域的 TerritoryType 行编号。</param>
    /// <param name="targetZoneIDs">目的地区域的 TerritoryType 行编号集合。</param>
    /// <returns>可到达区域及其路线费用。</returns>
    public static Dictionary<uint, uint> GetRouteCosts
    (
        uint              sourceZoneID,
        IEnumerable<uint> targetZoneIDs
    ) =>
        MovementManager.FindRouteCosts(sourceZoneID, targetZoneIDs);

    internal static unsafe Func<uint, AetheryteRecord, uint> CaptureTeleportCosts
    (
        IEnumerable<AetheryteRecord> records
    )
    {
        var territories = LuminaGetter.Get<TerritoryTypeTelepo>().ToDictionary
        (
            x => x.RowId,
            x => (X: (int)x.X, Y: (int)x.Y, Expansion: (uint)x.Expansion, Relay: x.Relay.RowId)
        );
        var relays = LuminaGetter.Get<TelepoRelay>().ToDictionary
        (
            x => x.RowId,
            x => (Width: x.Unknown_70, Hops: x.Relays.Select
                         (hop => (Enter: hop.EnterTerritory.RowId, Exit: hop.ExitTerritory.RowId, Cost: (uint)hop.Cost)).ToArray())
        );
        var destinations = records.ToDictionary
        (
            x => x,
            x => x.Group switch
            {
                254 => LuminaGetter.GetRowOrDefault<Aetheryte>(70).Territory.RowId,
                253 => LuminaGetter.GetRowOrDefault<Aetheryte>(175).Territory.RowId,
                _   => x.GetData().Territory.RowId
            }
        );
        var free      = new HashSet<ushort>();
        var favorites = new HashSet<ushort>();
        var home      = 0U;
        var player    = PlayerState.Instance();

        if (GameState.IsLoggedIn &&
            player != null)
        {
            foreach (var id in player->FreeAetheryteIds)
                free.Add(id);

            foreach (var id in player->FavouriteAetherytes)
                favorites.Add(id);

            home = player->HomeAetheryteId;
        }

        return (source, record) =>
        {
            var cost = GetBaseCost(source, destinations[record]);

            if (record.Group is 253 or 254 ||
                !record.IsAetheryte)
                return cost;

            if (free.Contains((ushort)record.RowID))
                return 0;

            if (favorites.Contains((ushort)record.RowID))
                return cost / 2;

            if (record.RowID == home)
                return cost;

            return record.IsHouse ?
                       cost / 4 :
                       cost;
        };

        uint GetBaseCost(uint source, uint target)
        {
            if (!territories.TryGetValue(source, out var current)     ||
                !territories.TryGetValue(target, out var destination) ||
                !relays.TryGetValue(current.Relay, out var relay))
                return 999;

            int cost;

            if (current.Relay == destination.Relay)
                cost = CalcDistanceCost
                (
                    (current.X, current.Y, current.Expansion),
                    (destination.X, destination.Y, destination.Expansion),
                    relay.Width
                );
            else
            {
                if (destination.Relay >= relay.Hops.Length)
                    return 999;

                var hop = relay.Hops[destination.Relay];

                if (hop.Enter == 0                                     ||
                    hop.Exit  == 0                                     ||
                    !territories.TryGetValue(hop.Enter, out var enter) ||
                    !territories.TryGetValue(hop.Exit,  out var exit)  ||
                    !relays.TryGetValue(exit.Relay, out var exitRelay))
                    return 999;

                cost = CalcDistanceCost
                       (
                           (current.X, current.Y, current.Expansion),
                           (enter.X, enter.Y, enter.Expansion),
                           relay.Width
                       )             +
                       (int)hop.Cost +
                       CalcDistanceCost
                       (
                           (exit.X, exit.Y, exit.Expansion),
                           (destination.X, destination.Y, destination.Expansion),
                           exitRelay.Width
                       );
            }

            return (uint)(cost > 1000 ?
                              ((cost - 1000) / 2) + 1000 :
                              cost);
        }
    }

    /// <summary>
    ///     计算单段交通的金币费用，以太传送应用免费、收藏及住宅折扣，区域事件使用其声明的费用。
    /// </summary>
    /// <param name="hop">需要计费的单段交通。</param>
    /// <param name="sourceZoneID">该段交通的出发区域编号。</param>
    /// <returns>该段交通的金币费用；过图线及城内以太网络免费。</returns>
    public static uint GetHopCost(ZoneHop hop, uint sourceZoneID) =>
        hop switch
        {
            AetheryteHop aetheryte => GetTeleportCost
            (
                aetheryte.Record.GetData(),
                sourceZoneID,
                aetheryte.Record.RowID,
                aetheryte.Record.Group,
                aetheryte.Record.IsAetheryte,
                aetheryte.Record.IsHouse
            ),
            EventHop transport => transport.Method.Cost,
            _                  => 0
        };

    /// <summary>
    ///     计算两个 Territory 之间的基础距离费用 (CalcTeleportDistanceCost)
    /// </summary>
    /// <param name="curRow">当前 Territory 的 TerritoryTypeTelepo 行</param>
    /// <param name="targetRow">目标 Territory 的 TerritoryTypeTelepo 行</param>
    /// <param name="worldWrapWidth">世界环绕半宽 (TelepoRelay.Unknown_70)</param>
    /// <returns>基础距离费用 (未经过软上限和除数)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalcDistanceCost
    (
        TerritoryTypeTelepo curRow,
        TerritoryTypeTelepo targetRow,
        uint                worldWrapWidth
    ) =>
        CalcDistanceCost
        (
            (curRow.X, curRow.Y, curRow.Expansion),
            (targetRow.X, targetRow.Y, targetRow.Expansion),
            worldWrapWidth
        );

    private static int CalcDistanceCost
    (
        (int X, int Y, uint Expansion) curRow,
        (int X, int Y, uint Expansion) targetRow,
        uint                           worldWrapWidth
    )
    {
        // Step 1: 计算 X 距离 (带世界环绕)
        var dx = Math.Abs(curRow.X - targetRow.X);
        if (dx > worldWrapWidth && dx < 2 * worldWrapWidth)
            dx = (int)(2 * worldWrapWidth) - dx;

        // Step 2: 计算 Y 距离 (无环绕)
        var dy = Math.Abs(curRow.Y - targetRow.Y);

        // Step 3: 欧几里得距离
        var dist = MathF.Sqrt((dx * dx) + (dy * dy));

        // Step 4: 大版本乘数
        // Expansion >= 400 表示至少一方是 3.0+ 扩展 → 乘数 = exp1 + exp2 + 600
        // 否则 (都是 2.x ARR+HW) → 乘数 = 1000
        var expSum = curRow.Expansion + targetRow.Expansion;
        var scaled = expSum >= 400 ?
                         expSum + 600 :
                         1000u;

        // Step 5: raw = round(scaled * dist / 1000)
        // 游戏使用定点数除法魔数 0x20C49BA5E353F7CF ≈ 2^71/1000
        var raw = (int)MathF.Round(scaled * dist / 1000f);

        // Step 6: cost = round((raw + 500) / 5)
        // 游戏使用魔数 0x6666666666666667 做除以 5
        return (int)MathF.Round((raw + 500f) / 5f);
    }

    /// <summary>
    ///     计算从当前 Territory 传送到目标以太之光的原始费用
    ///     不包含收藏/免费/住宅折扣 (divisor=1, multiplier=100)
    /// </summary>
    /// <param name="targetAetheryteRow">目标以太之光的 Aetheryte 行</param>
    /// <param name="zoneID">当前 TerritoryTypeID (来自 GameMain.CurrentTerritoryTypeId)</param>
    /// <returns>原始传送费用 (Gil)</returns>
    public static uint GetBaseTeleportCost(Aetheryte targetAetheryteRow, uint zoneID)
    {
        var targetZoneID = targetAetheryteRow.Territory.RowId;

        // 获取两个 Territory 的 TerritoryTypeTelepo 行
        if (!LuminaGetter.TryGetRow(zoneID,       out TerritoryTypeTelepo curRow) ||
            !LuminaGetter.TryGetRow(targetZoneID, out TerritoryTypeTelepo targetRow))
            return 999;

        // 获取当前 Relay 行
        if (!LuminaGetter.TryGetRow(curRow.Relay.RowId, out TelepoRelay curRelayRow))
            return 999;

        int cost;

        if (curRow.Relay.RowId == targetRow.Relay.RowId)
        {
            // 同一 Relay 内 → 直接计算
            cost = CalcDistanceCost(curRow, targetRow, curRelayRow.Unknown_70);
        }
        else
        {
            // 跨 Relay → 用目标 Relay ID 在当前 Relay 的跳板表中查找入口/出口
            var relayIdx = (int)targetRow.Relay.RowId;
            if (relayIdx is < 0 or > 8)
                return 999;

            var hop            = curRelayRow.Relays[relayIdx];
            var enterTerritory = hop.EnterTerritory.RowId;
            var exitTerritory  = hop.ExitTerritory.RowId;
            var hopCost        = hop.Cost;

            if (enterTerritory == 0 || exitTerritory == 0)
                return 999;

            if (!LuminaGetter.TryGetRow(enterTerritory, out TerritoryTypeTelepo enterRow) ||
                !LuminaGetter.TryGetRow(exitTerritory,  out TerritoryTypeTelepo exitRow))
                return 999;

            // 出口 Territory 所在 Relay 用于第二段计算
            if (!LuminaGetter.TryGetRow(exitRow.Relay.RowId, out TelepoRelay exitRelayRow))
                return 999;

            // 第一段: 当前 → 入口 Territory (在当前 Relay 内, 用 curRelayRow.Unknown_70)
            var cost1 = CalcDistanceCost(curRow, enterRow, curRelayRow.Unknown_70);

            // 第二段: 出口 Territory → 目标 (在目标 Relay 内, 用 exitRelayRow.Unknown_70)
            var cost2 = CalcDistanceCost(exitRow, targetRow, exitRelayRow.Unknown_70);

            cost = cost1 + hopCost + cost2;
        }

        // 软上限: 超过 1000 的部分减半
        if (cost > 1000)
            cost = ((cost - 1000) / 2) + 1000;

        // divisor=1, multiplier=100 → 直接返回
        return (uint)cost;
    }

    public static unsafe uint GetTeleportCost
    (
        Aetheryte targetAetheryteRow,
        uint      sourceZoneID,
        uint      aetheryteRowID,
        byte      aethernetGroup,
        bool      isAetheryte,
        bool      isHouse
    )
    {
        var baseCost = GetBaseTeleportCost(targetAetheryteRow, sourceZoneID);

        switch (aethernetGroup)
        {
            case 254:
            {
                var ishgardData = LuminaGetter.GetRow<Aetheryte>(70).GetValueOrDefault();
                return GetBaseTeleportCost(ishgardData, sourceZoneID);
            }
            case 253:
            {
                var burrowData = LuminaGetter.GetRow<Aetheryte>(175).GetValueOrDefault();
                return GetBaseTeleportCost(burrowData, sourceZoneID);
            }
        }

        if (!isAetheryte)
            return baseCost;

        if (!GameState.IsLoggedIn)
            return baseCost;

        var instance = PlayerState.Instance();
        if (instance == null) return baseCost;

        if (instance->FreeAetheryteIds.Contains((ushort)aetheryteRowID))
            return 0;

        if (instance->FavouriteAetherytes.Contains((ushort)aetheryteRowID))
            return baseCost / 2;

        if (instance->HomeAetheryteId == aetheryteRowID)
            return baseCost;

        if (isHouse)
            return baseCost / 4;

        return baseCost;
    }
}

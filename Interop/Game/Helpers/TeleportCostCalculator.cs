using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.Helpers;

/// <summary>
///     传送费用本地计算器
/// </summary>
public static class TeleportCostCalculator
{
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
        var expSum = (uint)(curRow.Expansion + targetRow.Expansion);
        var scaled = expSum >= 400 ? expSum + 600 : 1000u;

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

        if (instance->FreeAetheryteId     == aetheryteRowID ||
            instance->FreeAetherytePSPlus == aetheryteRowID ||
            instance->FreeAetheryteNSO    == aetheryteRowID)
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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.AetheryteRecord.Data;
using OmenTools.Info.Game.AetheryteRecord.Enums;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using TinyPinyin;
using Map = Lumina.Excel.Sheets.Map;

namespace OmenTools.Info.Game.AetheryteRecord;

/// <summary>
///     Group: 255 冒险者住宅区, 254 天穹街, 253 宇宙探索
///     有需要请自行实现保存
/// </summary>
public record AetheryteRecord
(
    uint    RowID,
    byte    SubIndex,
    byte    Group,
    uint    Version,
    uint    ZoneID,
    uint    MapID,
    bool    IsAetheryte,
    Vector3 Position,
    string  Name
)
{
    private static readonly HashSet<byte> ValidGroups =
        LuminaGetter.Get<Aetheryte>().Select(x => x.AethernetGroup).ToHashSet();

    public AetheryteRecordState State { get; private set; }
    public uint                 Cost  { get; private set; }

    public string RegionName
    {
        get
        {
            if (field == null)
            {
                var name = Group switch
                {
                    5   => LuminaWrapper.GetZonePlaceName(144),       // 金碟
                    255 => LuminaWrapper.GetAddonText(8495),          // 冒险者住宅区
                    254 => GetZone().PlaceName.Value.Name.ToString(), // 天穹街
                    _   => GetZone().PlaceNameZone.Value.Name.ToString()
                };

                if (ZoneID == 250)
                    name = LuminaWrapper.GetPlaceName(22); // 狼狱停船场

                field = name;
            }

            return field;
        }
    }

    public virtual bool Equals(AetheryteRecord? other) =>
        other?.RowID == RowID;

    public unsafe bool IsUnlocked() =>
        UIState.Instance()->IsAetheryteUnlocked(RowID);

    public void Update()
    {
        if (DService.Instance().ObjectTable.LocalPlayer == null ||
            DService.Instance().Condition.IsBetweenAreas        ||
            !ValidGroups.Contains(Group) && Group != 255 && Group != 254)
            return;

        var info = GetAetheryteState(this);
        State = info.State;
        Cost  = info.Cost;
    }

    public static AetheryteRecord? Parse(Aetheryte data)
    {
        if (data.RowId <= 1) return null;
        var map = data.Map.RowId != 0 ? data.Map.Value : data.Territory.Value.Map.Value;

        var name = data.IsAetheryte
                       ? data.PlaceName.Value.Name.ToString()
                       : data.AethernetName.Value.Name.ToString();
        if (string.IsNullOrWhiteSpace(name)) return null;

        var subIndex = 0;

        // 城内以太之晶才需要排序, 以太之光肯定始终 index 为 0
        if (data is { IsAetheryte: false, AethernetGroup: > 0 })
        {
            // 先全选
            var all = LuminaGetter.Get<Aetheryte>()
                                  .Where(x => x.AethernetGroup == data.AethernetGroup)
                                  .ToList();

            // 挑出飞艇坪等“其他”里也始终最后的 (同一区域但不同地图)
            var special = all.Where(x => x is { IsAetheryte: false, Map.RowId: > 0 })
                             .OrderBy(x => x.Territory.RowId)
                             .ThenBy(x => x.RowId)
                             .ToList();
            all = all.ExceptBy(special.Select(x => x.RowId), x => x.RowId).ToList();

            // 挑出这一组的大水晶
            var aetheryteMain = all.FirstOrDefault(x => x.IsAetheryte);

            // 挑出飞到野外的小水晶, 在“其他”里
            var outdoors = all.Where(x => LuminaGetter.TryGetRow<Level>(x.Level[0].RowId, out _))
                              .OrderBy(x => x.Territory.RowId)
                              .ThenBy(x => x.RowId)
                              .ToList();
            all = all.ExceptBy(outdoors.Select(x => x.RowId), x => x.RowId).ToList();

            // 剩下的就各找各的先 Zone 再 RowID 排序
            var left = all.Where(x => !x.IsAetheryte)
                          .OrderBy(x => x.RowId)
                          // .ThenBy(x => x.RowId)
                          .ToList();

            // 最终成品
            var finalList = new List<Aetheryte> { aetheryteMain };
            // 再剩下普通的
            foreach (var aetheryte in left)
                finalList.Add(aetheryte);
            // 再野外的
            foreach (var aetheryte in outdoors)
                finalList.Add(aetheryte);
            // 最后特殊的
            foreach (var aetheryte in special)
                finalList.Add(aetheryte);

            subIndex = finalList.IndexOf(x => x.RowId == data.RowId);
        }

        Vector3 position = default;

        if (AetheryteRecords.NormalPositions.TryGetValue(data.RowId, out var redirectedPosition))
            position = redirectedPosition;
        else if (LuminaGetter.TryGetRow<Level>(data.Level[0].RowId, out var level))
            position = level.GetPosition();
        else
        {
            position = data.Territory.Value
                           .GetMapMarkers()
                           .Where(x => x.DataType is 3 or 4)
                           .Where(x => TryParseName(x, out var markerName) && markerName == name)
                           .Select(x => PositionHelper.TextureToWorld(new(x.X, x.Y), map).ToVector3(0))
                           .FirstOrDefault();
        }

        var version = data.Territory.Value.ExVersion.RowId;
        if (AetheryteRecords.NormalVersions.TryGetValue(data.RowId, out var redirectedVersion))
            version = redirectedVersion;

        var record = new AetheryteRecord
        (
            data.RowId,
            (byte)subIndex,
            data.AethernetGroup,
            version,
            data.Territory.RowId,
            map.RowId,
            data.IsAetheryte,
            position,
            name
        );

        return record;
    }

    public Aetheryte GetData() =>
        LuminaGetter.GetRow<Aetheryte>(RowID).GetValueOrDefault();

    public TerritoryType GetZone() =>
        LuminaGetter.GetRow<TerritoryType>(ZoneID).GetValueOrDefault();

    public Map GetMap() =>
        LuminaGetter.GetRow<Map>(MapID).GetValueOrDefault();

    public static bool TryParseName(MapMarker marker, [NotNullWhen(true)] out string? name)
    {
        name = null;

        if (marker.Icon == 0) return false;
        if (marker.DataType is not (3 or 4)) return false;

        switch (marker.DataType)
        {
            case 3:
                if (!LuminaGetter.TryGetRow<Aetheryte>(marker.DataKey.RowId, out var aetheryte)) return false;
                name = aetheryte.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;
                break;
            case 4:
                if (!LuminaGetter.TryGetRow<PlaceName>(marker.DataKey.RowId, out var aethernetNameRow)) return false;
                name = aethernetNameRow.Name.ToString() ?? string.Empty;
                break;
            default:
                return false;
        }

        return true;
    }

    public static unsafe (AetheryteRecordState State, uint Cost) GetAetheryteState(AetheryteRecord aetheryte)
    {
        if (DService.Instance().AetheryteList.Count == 0) return default;

        // 天穹街
        // 伊修加德基础层
        if (aetheryte.Group == 254)
            return (AetheryteRecordState.None, DService.Instance().AetheryteList.FirstOrDefault(x => x.AetheryteID == 70)?.GilCost ?? 0);
        
        // 宇宙探索
        // 最佳威兔洞
        if (aetheryte.Group == 253)
            return (AetheryteRecordState.None, DService.Instance().AetheryteList.FirstOrDefault(x => x.AetheryteID == 175)?.GilCost ?? 0);

        if (!aetheryte.IsAetheryte)
        {
            var mainAetheryte = DService.Instance().AetheryteList.FirstOrDefault(x => x.AetheryteData.Value.AethernetGroup == aetheryte.Group);
            if (mainAetheryte == null) return default;

            return (AetheryteRecordState.None, mainAetheryte.GilCost);
        }

        var serviceState = DService.Instance().AetheryteList.FirstOrDefault(x => x.AetheryteID == aetheryte.RowID);
        if (serviceState == null) return default;

        var cost = serviceState.GilCost;

        var instance = PlayerState.Instance();
        if (instance == null) return default;

        if (instance->FreeAetheryteId == aetheryte.RowID)
            return (AetheryteRecordState.Free, cost);

        if (instance->FreeAetherytePSPlus == aetheryte.RowID)
            return (AetheryteRecordState.FreePS, cost);
        
        if (instance->FreeAetheryteNSO == aetheryte.RowID)
            return (AetheryteRecordState.FreeNSO, cost);

        if (instance->FavouriteAetherytes.Contains((ushort)aetheryte.RowID))
            return (AetheryteRecordState.Favorite, cost);

        if (instance->HomeAetheryteId == aetheryte.RowID)
            return (AetheryteRecordState.Home, cost);

        if (serviceState.IsSharedHouse)
            return (AetheryteRecordState.SharedHouse, cost);

        if (serviceState.IsApartment)
            return (AetheryteRecordState.Apart, cost);

        return (AetheryteRecordState.None, cost);
    }

    public override string ToString()
    {
        var zoneName = GetZone().ExtractPlaceName();

        return $"AetheryteRecord_{RowID}_{Version + 2}.0_{SubIndex}_{zoneName}_{RegionName}_{Name}_" +
               $"{PinyinHelper.GetPinyin(zoneName, string.Empty)}_{PinyinHelper.GetPinyin(RegionName, string.Empty)}_{PinyinHelper.GetPinyin(Name, string.Empty)}";
    }

    public override int GetHashCode() =>
        HashCode.Combine(RowID, Name);
}

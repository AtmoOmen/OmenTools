using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.AetheryteRecord;
using OmenTools.Info.Game.AetheryteRecord.Data;
using OmenTools.Info.Lumina.ExtraSheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService.Abstractions;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.OmenService;

public class AetheryteRecordManager : OmenServiceBase<AetheryteRecordManager>
{
    public string HouseApartmentTemplate { get; set; } = "{0} {1} (第 {2} 区 {3} 号房间)";
    public string HouseEstateTemplate    { get; set; } = "{0} {1} (第 {2} 区 {3} 号)";
    public string HouseSharedName        { get; set; } = "共享房屋";
    public string HouseFreeCompanyName   { get; set; } = "部队房屋";
    public string HousePersonalName      { get; set; } = "个人房屋";

    public Dictionary<string, List<AetheryteRecord>> Records { get; private set; } = [];

    public IEnumerable<AetheryteRecord> AllRecords
    {
        get
        {
            foreach (var list in Records.Values)
            foreach (var record in list)
                yield return record;
        }
    }

    public AetheryteRecord? GetNearestAetheryte
    (
        uint    zoneID,
        Vector3 pos
    )
    {
        var validAetherytes = AllRecords
                              .Where(x => AetheryteRecords.AethernetGroups.Contains(x.Group) && x.ZoneID == zoneID)
                              .OrderBy(x => Vector3.DistanceSquared(x.Position, pos))
                              .ToList();
        return validAetherytes.Count == 0 ? null : validAetherytes.FirstOrDefault();
    }
    
    protected override void Init()
    {
        DService.Instance().ClientState.TerritoryChanged += OnZoneChanged;
        GameState.Instance().Login += OnLogin;
        
        OnLogin();
    }

    protected override void Uninit()
    {
        DService.Instance().ClientState.TerritoryChanged -= OnZoneChanged;
        GameState.Instance().Login                       -= OnLogin;
    }

    private void OnLogin()
    {
        if (!GameState.IsLoggedIn) return;
        
        BuildRecords();
        RefreshRecords();
    }

    private void OnZoneChanged(uint zone)
    {
        if (GameState.ContentFinderCondition != 0 ||
            !GameState.IsLoggedIn)
            return;

        RefreshRecords();
    }

    private void BuildRecords()
    {
        Records = [];
        
        foreach (var data in LuminaGetter.Get<Aetheryte>()
                                         .OrderBy(x => x.AethernetGroup)
                                         .ThenBy(x => x.Territory.RowId))
        {
            var record = AetheryteRecord.Parse(data);
            if (record == null || !record.IsUnlocked())
                continue;

            // 金碟游乐场
            if (record.Group == 5)
            {
                Records.TryAdd(VERSION_OTHER, []);
                Records[VERSION_OTHER].Add(record);
            }
            else if (record.Version == 0)
            {
                var regionRow = record.GetZone().PlaceNameRegion.Value;

                // 拉诺西亚 / 萨纳兰 / 黑衣森林
                var regionName = regionRow.RowId is 22 or 23 or 24
                                     ? record.GetZone().PlaceNameRegion.Value.Name.ToString()
                                     : VERSION_OTHER; // 摩杜纳

                Records.TryAdd(regionName, []);
                Records[regionName].Add(record);
            }
            else
            {
                var versionName = $"{record.Version + 2}.0";

                Records.TryAdd(versionName, []);
                Records[versionName].Add(record);
            }
        }

        BuildFirmamentRecords();
        BuildCosmicExplorationRecords();
        BuildHouseRecords();
    }

    private void RefreshRecords()
    {
        foreach (var record in AllRecords)
            record.Update();
    }

    private void BuildHouseRecords()
    {
        var housingMarkers = LuminaGetter.GetSub<HousingMapMarkerInfo>()
                                         .SelectMany(x => x)
                                         .Where(x => x.Map.ValueNullable != null)
                                         .ToList();

        // 部队房
        var fcInfo = HousingManager.GetOwnedHouseId(EstateType.FreeCompanyEstate);
        if (fcInfo.Id != INVALID_HOUSE_ID)
        {
            var zoneID = fcInfo.TerritoryTypeId;
            var data = LuminaGetter.Get<Aetheryte>()
                                   .FirstOrDefault(x => x.Territory.RowId == zoneID && x.PlaceName.RowId == 1145);
            var marker = housingMarkers.Where(x => x.Map.Value.TerritoryType.RowId == zoneID)
                                       .FirstOrDefault(x => x.SubrowId             == fcInfo.PlotIndex);

            if (data.RowId != 0 && marker.RowId != 0)
            {
                var territoryName = data.Territory.Value.ExtractPlaceName();
                var name = string.Format
                (
                    HouseEstateTemplate,
                    territoryName,
                    HouseFreeCompanyName,
                    fcInfo.WardIndex + 1,
                    fcInfo.PlotIndex + 1
                );

                var record = new AetheryteRecord
                (
                    data.RowId,
                    0,
                    data.AethernetGroup,
                    0,
                    zoneID,
                    marker.Map.RowId,
                    true,
                    new(marker.X, marker.Y, marker.Z),
                    name
                );

                Records.TryAdd(VERSION_OTHER, []);
                Records[VERSION_OTHER].Add(record);
            }
        }

        // 个人房
        var personalInfo = HousingManager.GetOwnedHouseId(EstateType.PersonalEstate);

        if (personalInfo.Id != INVALID_HOUSE_ID)
        {
            var zoneID = personalInfo.TerritoryTypeId;
            var data = LuminaGetter.Get<Aetheryte>()
                                   .FirstOrDefault(x => x.Territory.RowId == zoneID && x.PlaceName.RowId == 1160);
            var marker = housingMarkers.Where(x => x.Map.Value.TerritoryType.RowId == zoneID)
                                       .FirstOrDefault(x => x.SubrowId             == personalInfo.PlotIndex);

            if (data.RowId != 0 && marker.RowId != 0)
            {
                var territoryName = data.Territory.Value.ExtractPlaceName();
                var name = string.Format
                (
                    HouseEstateTemplate,
                    territoryName,
                    HousePersonalName,
                    personalInfo.WardIndex + 1,
                    personalInfo.PlotIndex + 1
                );

                var record = new AetheryteRecord
                (
                    data.RowId,
                    0,
                    data.AethernetGroup,
                    0,
                    zoneID,
                    marker.Map.RowId,
                    true,
                    new(marker.X, marker.Y, marker.Z),
                    name
                );

                Records.TryAdd(VERSION_OTHER, []);
                Records[VERSION_OTHER].Add(record);
            }
        }

        // 公寓
        var aptBuilding = HousingManager.GetOwnedHouseId(EstateType.ApartmentBuilding);
        var aptRoom     = HousingManager.GetOwnedHouseId(EstateType.ApartmentRoom);

        if (aptBuilding.Id != INVALID_HOUSE_ID && aptRoom.Id != INVALID_HOUSE_ID)
        {
            var zoneID = aptBuilding.TerritoryTypeId;
            var data = LuminaGetter.Get<Aetheryte>()
                                   .FirstOrDefault(x => x.Territory.RowId == zoneID && x.PlaceName.RowId == 1160);
            var marker = housingMarkers.Where(x => x.Map.Value.TerritoryType.RowId == zoneID)
                                       .FirstOrDefault(x => x.SubrowId             == 60);

            if (data.RowId != 0 && marker.RowId != 0)
            {
                var territoryName = data.Territory.Value.ExtractPlaceName();
                var name = string.Format
                (
                    HouseApartmentTemplate,
                    territoryName,
                    LuminaWrapper.GetAddonText(6760),
                    aptBuilding.WardIndex + 1,
                    aptRoom.RoomNumber
                );

                var record = new AetheryteRecord
                (
                    data.RowId,
                    0,
                    data.AethernetGroup,
                    0,
                    zoneID,
                    marker.Map.RowId,
                    true,
                    new(marker.X, marker.Y, marker.Z),
                    name
                );

                Records.TryAdd(VERSION_OTHER, []);
                Records[VERSION_OTHER].Add(record);
            }
        }
    }

    private void BuildFirmamentRecords()
    {
        var territory = LuminaGetter.GetRowOrDefault<TerritoryType>(886);
        var map       = territory.Map.Value;

        foreach (var (index, position) in AetheryteRecords.FirmamentPositions)
        {
            var hwdTranspoint = LuminaGetter.GetRowOrDefault<HwdTranspoint>(index + 1);
            var name          = hwdTranspoint.Text.ToString();

            var record = new AetheryteRecord
            (
                70,
                (byte)index,
                254,
                1,
                886,
                map.RowId,
                false,
                position,
                name
            );

            Records.TryAdd("3.0", []);
            Records["3.0"].Add(record);
        }
    }

    private void BuildCosmicExplorationRecords()
    {
        byte index = 0;

        foreach (var (territoryID, positions) in AetheryteRecords.CosmicExplorationPositions)
        {
            var territory   = LuminaGetter.GetRowOrDefault<TerritoryType>(territoryID);
            var map         = territory.Map.Value;
            var version     = territory.ExVersion.RowId;
            var versionName = $"{version + 2}.0";
            var offset      = AetheryteRecords.CosmicPlaceNameOffsets[territoryID];

            foreach (var position in positions)
            {
                var placeName = LuminaWrapper.GetPlaceName(index + offset);
                var record = new AetheryteRecord
                (
                    175,
                    index,
                    253,
                    version,
                    territoryID,
                    map.RowId,
                    false,
                    position,
                    placeName
                );

                Records.TryAdd(versionName, []);
                Records[versionName].Add(record);

                index++;
            }
        }
    }

    #region 常量

    private const ulong INVALID_HOUSE_ID = 0xFFFFFFFFFFFFFFFF;

    private static readonly string VERSION_OTHER = LuminaWrapper.GetAddonText(832);

    #endregion
}

using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using OmenTools.Info.Game.AetheryteRecord;
using OmenTools.Info.Game.AetheryteRecord.Data;
using OmenTools.Info.Lumina.ExtraSheets;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public class AetheryteRecordManager
(
    GameState       gameState,
    MovementManager movementManager
) : OmenServiceBase<AetheryteRecordManager>
{
    public string HouseApartmentTemplate { get; set; } = "{houseType}（{zoneName}第{ward}区{plot}号房间）";
    public string HouseEstateTemplate    { get; set; } = "{houseType}（{zoneName}第{ward}区{plot}号）";
    public string HouseSharedName        { get; set; } = "共享房屋";
    public string HouseFreeCompanyName   { get; set; } = "部队房屋";
    public string HousePersonalName      { get; set; } = "个人房屋";

    public Dictionary<string, List<AetheryteRecord>> Records { get; private set; } = [];

    /// <remarks>
    ///     如果需要获取公寓，请使用 <see cref="EstateType.ApartmentBuilding" /> 来筛选。
    /// </remarks>
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
        Vector3 pos,
        bool    excludeAethernet = false
    )
    {
        var validAetherytes = AllRecords
                              .Where
                              (x => AetheryteRecords.AethernetGroups.Contains(x.Group) &&
                                    x.ZoneID == zoneID                                 &&
                                    x.IsUnlocked()
                              )
                              .Where(x => !excludeAethernet || x.IsAetheryte)
                              .OrderBy(x => Vector3.DistanceSquared(x.Position, pos))
                              .ToList();
        return validAetherytes.Count == 0 ?
                   null :
                   validAetherytes.FirstOrDefault();
    }

    protected override void Init()
    {
        if (GameState.IsLoggedIn)
        {
            BuildRecords();
            RefreshRecords(true);
        }

        IClientState.Instance().TerritoryChanged += OnZoneChanged;
        gameState.Login                          += OnLogin;
        gameState.WorldChanged                   += OnWorldChanged;
    }

    protected override void Uninit()
    {
        IClientState.Instance().TerritoryChanged -= OnZoneChanged;
        gameState.Login                          -= OnLogin;
        gameState.WorldChanged                   -= OnWorldChanged;
    }

    private void OnWorldChanged
    (
        uint obj
    )
    {
        BuildRecords();
        RefreshRecords(true);
    }

    private void OnLogin()
    {
        if (!GameState.IsLoggedIn) return;

        BuildRecords();
        RefreshRecords(true);
    }

    private void OnZoneChanged
    (
        uint zone
    )
    {
        if (GameState.ContentFinderCondition != 0 ||
            !GameState.IsLoggedIn)
            return;

        RefreshRecords();
    }

    private void BuildRecords()
    {
        Records = [];

        var pendingRecords = LuminaGetter.Get<Aetheryte>()
                                         .OrderBy(x => x.AethernetGroup)
                                         .ThenBy(x => x.Territory.RowId);

        foreach (var data in pendingRecords)
        {
            var record = AetheryteRecord.Parse(data);
            if (record == null ||
                !record.IsUnlocked())
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
                var regionName = regionRow.RowId is 22 or 23 or 24 ?
                                     record.GetZone().PlaceNameRegion.Value.Name.ToString() :
                                     VERSION_OTHER; // 摩杜纳

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
        BuildEventTransportRecords();
    }

    private void BuildEventTransportRecords()
    {
        var existingZones = AllRecords.Select(x => x.ZoneID).ToHashSet();
        var targetZones = movementManager.EventTransportations
                                         .SelectMany(x => x.TargetZones)
                                         .Distinct()
                                         .Order();

        foreach (var zoneID in targetZones)
        {
            if (existingZones.Contains(zoneID))
                continue;

            var territory = LuminaGetter.GetRowOrDefault<TerritoryType>(zoneID);
            var name      = territory.PlaceName.Value.Name.ToString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var version = zoneID == 1055 ?
                              4U :
                              territory.ExVersion.RowId;
            var record = new AetheryteRecord
            (
                zoneID,
                0,
                252,
                version,
                zoneID,
                territory.Map.RowId,
                false,
                Vector3.Zero,
                name
            )
            {
                RegionName = territory.TerritoryIntendedUse.RowId switch
                {
                    41 or 48 or 61        => territory.ContentFinderCondition.Value.ContentType.Value.Name.ToString(),
                    _ when zoneID == 1055 => name,
                    _                     => territory.PlaceNameZone.Value.Name.ToString()
                }
            };

            var category = $"{version + 2}.0";

            if (version == 0)
            {
                var region = territory.PlaceNameRegion.Value;
                category = region.RowId is 22 or 23 or 24 ?
                               region.Name.ToString() :
                               VERSION_OTHER;
            }

            Records.TryAdd(category, []);
            Records[category].Add(record);
        }

        foreach (var records in Records.Values)
        {
            var transports = records.Where(x => x.Group == 252)
                                    .OrderBy(x => x.RegionName, StringComparer.Ordinal)
                                    .ThenBy(x => x.ZoneID)
                                    .ToList();
            records.RemoveAll(x => x.Group == 252);
            records.AddRange(transports);
        }
    }

    private void RefreshRecords(bool calculateRouteCosts = false)
    {
        foreach (var record in AllRecords.Where(x => x.Group is not (252 or 253 or 254)))
            record.Update();

        foreach (var record in AllRecords.Where(x => x.Group is 252 or 253 or 254))
        {
            record.Update
            (
                calculateRouteCosts ?
                    TeleportCostCalculator.GetRouteCost(GameState.TerritoryType, record.ZoneID) :
                    uint.MaxValue
            );
        }
    }

    private void BuildHouseRecords()
    {
        var housingMarkers = LuminaGetter.GetSub<HousingMapMarkerInfo>()
                                         .SelectMany(x => x)
                                         .Where(x => x.Map.ValueNullable != null)
                                         .ToList();

        // 公寓
        var aptBuilding = HousingManager.GetOwnedHouseId(EstateType.ApartmentBuilding);
        var aptRoom     = HousingManager.GetOwnedHouseId(EstateType.ApartmentRoom);

        if (aptBuilding.Id != INVALID_HOUSE_ID && aptRoom.Id != INVALID_HOUSE_ID)
        {
            TryAddHouseRecord
            (
                housingMarkers,
                EstateType.ApartmentBuilding,
                -1,
                aptBuilding.TerritoryTypeId,
                GetApartmentAetheryteID,
                60,
                128, // 固定值
                HouseApartmentTemplate,
                [aptBuilding.WardIndex + 1, aptRoom.RoomNumber],
                LuminaWrapper.GetAddonText(6760)
            );
        }

        // 个人房
        var personalInfo = HousingManager.GetOwnedHouseId(EstateType.PersonalEstate);

        if (personalInfo.Id != INVALID_HOUSE_ID)
        {
            TryAddHouseRecord
            (
                housingMarkers,
                EstateType.PersonalEstate,
                -1,
                personalInfo.TerritoryTypeId,
                GetApartmentAetheryteID,
                personalInfo.PlotIndex,
                0,
                HouseEstateTemplate,
                [personalInfo.WardIndex + 1, personalInfo.PlotIndex + 1],
                HousePersonalName
            );
        }

        // 部队房
        var fcInfo = HousingManager.GetOwnedHouseId(EstateType.FreeCompanyEstate);

        if (fcInfo.Id != INVALID_HOUSE_ID)
        {
            TryAddHouseRecord
            (
                housingMarkers,
                EstateType.FreeCompanyEstate,
                -1,
                fcInfo.TerritoryTypeId,
                GetFreeCompanyAetheryteID,
                fcInfo.PlotIndex,
                0,
                HouseEstateTemplate,
                [fcInfo.WardIndex + 1, fcInfo.PlotIndex + 1],
                HouseFreeCompanyName
            );
        }

        // 共享房屋
        for (var i = 0; i < 2; i++)
        {
            var sharedInfo = HousingManager.GetOwnedHouseId(EstateType.SharedEstate, i);
            if (sharedInfo.Id == INVALID_HOUSE_ID)
                continue;

            TryAddHouseRecord
            (
                housingMarkers,
                EstateType.SharedEstate,
                i,
                sharedInfo.TerritoryTypeId,
                GetApartmentAetheryteID,
                sharedInfo.PlotIndex,
                (byte)(i + 1),
                HouseEstateTemplate,
                [sharedInfo.WardIndex + 1, sharedInfo.PlotIndex + 1],
                HouseSharedName
            );
        }
    }

    private void TryAddHouseRecord
    (
        List<HousingMapMarkerInfo> housingMarkers,
        EstateType                 houseType,
        int                        shareHouseIndex,
        uint                       zoneID,
        Func<uint, uint>           getAetheryteID,
        uint                       markerSubrowID,
        byte                       subIndex,
        string                     nameTemplate,
        object[]                   nameArgs,
        string                     houseTypeName
    )
    {
        var aetheryteID = getAetheryteID(zoneID);
        var data        = LuminaGetter.GetRowOrDefault<Aetheryte>(aetheryteID);
        var marker = housingMarkers.Where(x => x.Map.Value.TerritoryType.RowId == zoneID)
                                   .FirstOrDefault(x => x.SubrowId             == markerSubrowID);
        if (data.RowId   == 0 ||
            marker.RowId == 0)
            return;

        var territoryName = data.Territory.Value.ExtractPlaceName();
        var name = string.FormatICUText
        (
            nameTemplate,
            new Dictionary<string, object>
            {
                ["zoneName"]  = territoryName,
                ["houseType"] = houseTypeName,
                ["ward"]      = nameArgs[0],
                ["plot"]      = nameArgs[1]
            }
        );

        var record = new AetheryteRecord
        (
            data.RowId,
            subIndex,
            data.AethernetGroup,
            0,
            zoneID,
            marker.Map.RowId,
            true,
            new(marker.X, marker.Y, marker.Z),
            name
        )
        {
            IsHouse         = true,
            HouseType       = houseType,
            ShareHouseIndex = shareHouseIndex
        };

        Records.TryAdd(VERSION_OTHER, []);
        Records[VERSION_OTHER].Add(record);
    }

    // 逆向来的
    private static uint GetApartmentAetheryteID
    (
        uint zoneID
    ) =>
        zoneID switch
        {
            339 => 59U,
            340 => 60U,
            341 => 61U,
            641 => 97U,
            979 => 165U
        };

    // 逆向来的
    private static uint GetFreeCompanyAetheryteID
    (
        uint zoneID
    ) =>
        zoneID switch
        {
            339 => 56U,
            340 => 57U,
            341 => 58U,
            641 => 96U,
            979 => 164U
        };

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

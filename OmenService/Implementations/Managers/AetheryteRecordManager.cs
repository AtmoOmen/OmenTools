using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
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

    private TaskHelper? taskHelper;

    private const ulong INVALID_HOUSE_ID = 0xFFFFFFFFFFFFFFFF;

    protected override void Init()
    {
        taskHelper = new() { TimeoutMS = 60_000 };

        DService.Instance().ClientState.TerritoryChanged += OnZoneChanged;
        OnZoneChanged(0);

        GameState.Instance().Login += OnLogin;
    }

    protected override void Uninit()
    {
        DService.Instance().ClientState.TerritoryChanged -= OnZoneChanged;
        GameState.Instance().Login                       -= OnLogin;

        taskHelper?.Dispose();
        taskHelper = null;
    }

    private void OnLogin() =>
        OnZoneChanged(0);

    private void OnZoneChanged(uint zone)
    {
        taskHelper.Abort();

        if (GameState.ContentFinderCondition != 0 ||
            !GameState.IsLoggedIn)
            return;

        taskHelper.Enqueue(() => UIModule.IsScreenReady());
        taskHelper.Enqueue(RefreshRecords);
    }

    private void RefreshRecords()
    {
        Records = [];

        foreach (var data in LuminaGetter.Get<Aetheryte>()
                                         .OrderBy(x => x.AethernetGroup)
                                         .ThenBy(x => x.Territory.RowId))
        {
            var record = AetheryteRecord.Parse(data);
            if (record == null       ||
                !record.IsUnlocked() ||
                record.IsHouse)
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

        RefreshFirmamentRecords();
        RefreshCosmicExplorationRecords();
        RefreshHouseRecords();

        foreach (var record in AllRecords)
            record.Update();
    }

    private void RefreshFirmamentRecords()
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

    private void RefreshCosmicExplorationRecords()
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

    private void RefreshHouseRecords()
    {
        var allHousingMarkers = LuminaGetter.GetSub<HousingMapMarkerInfo>()
                                            .SelectMany(x => x)
                                            .Where(x => x.Map.ValueNullable != null)
                                            .ToList();

        Records.TryAdd(VERSION_OTHER, []);

        foreach (var aetheryte in DService.Instance().AetheryteList)
        {
            if (!LuminaGetter.TryGetRow<Aetheryte>(aetheryte.AetheryteID, out var aetheryteRow)) continue;
            if (aetheryteRow.PlaceName.RowId is not (1145 or 1160)) continue;

            var housingMarkers = allHousingMarkers
                                 .Where(x => x.Map.Value.TerritoryType.RowId == aetheryteRow.Territory.RowId)
                                 .ToList();

            var territoryName = aetheryteRow.Territory.Value.ExtractPlaceName();

            if (aetheryte.IsApartment)
            {
                var aptHouseInfo = HousingManager.GetOwnedHouseId(EstateType.ApartmentBuilding);
                var aptRoomInfo  = HousingManager.GetOwnedHouseId(EstateType.ApartmentRoom);
                if (aptHouseInfo.Id == INVALID_HOUSE_ID || aptRoomInfo.Id == INVALID_HOUSE_ID) continue;

                var aptMarker = housingMarkers.FirstOrDefault(x => x.SubrowId == 60);
                if (aptMarker.RowId == 0) continue;

                Records[VERSION_OTHER].Add
                (
                    new AetheryteRecord
                    (
                        aetheryte.AetheryteID,
                        aetheryte.SubIndex,
                        aetheryteRow.AethernetGroup,
                        0,
                        aetheryte.TerritoryID,
                        aptMarker.Map.RowId,
                        true,
                        new(aptMarker.X, aptMarker.Y, aptMarker.Z),
                        string.Format
                        (
                            HouseApartmentTemplate,
                            territoryName,
                            LuminaWrapper.GetAddonText(6760),
                            aptHouseInfo.WardIndex + 1,
                            aptRoomInfo.RoomNumber
                        )
                    )
                );
                continue;
            }

            if (aetheryte.IsSharedHouse)
            {
                var sharedHouseMarker = housingMarkers.FirstOrDefault(x => x.SubrowId == aetheryte.Plot);
                if (sharedHouseMarker.RowId == 0) continue;

                Records[VERSION_OTHER].Add
                (
                    new AetheryteRecord
                    (
                        aetheryte.AetheryteID,
                        aetheryte.SubIndex,
                        aetheryteRow.AethernetGroup,
                        0,
                        aetheryte.TerritoryID,
                        sharedHouseMarker.Map.RowId,
                        true,
                        new(sharedHouseMarker.X, sharedHouseMarker.Y, sharedHouseMarker.Z),
                        string.Format
                        (
                            HouseEstateTemplate,
                            territoryName,
                            HouseSharedName,
                            aetheryte.Ward,
                            aetheryte.Plot
                        )
                    )
                );
                continue;
            }


            switch (aetheryteRow.PlaceName.RowId)
            {
                // 部队房屋
                case 1145:
                {
                    var fcHouseInfo = HousingManager.GetOwnedHouseId(EstateType.FreeCompanyEstate);
                    if (fcHouseInfo.Id == INVALID_HOUSE_ID) continue;

                    var fcMarker = housingMarkers.FirstOrDefault(x => x.SubrowId == fcHouseInfo.PlotIndex);
                    if (fcMarker.RowId == 0) continue;

                    Records[VERSION_OTHER].Add
                    (
                        new AetheryteRecord
                        (
                            aetheryte.AetheryteID,
                            aetheryte.SubIndex,
                            aetheryteRow.AethernetGroup,
                            0,
                            aetheryte.TerritoryID,
                            fcMarker.Map.RowId,
                            true,
                            new(fcMarker.X, fcMarker.Y, fcMarker.Z),
                            string.Format
                            (
                                HouseEstateTemplate,
                                territoryName,
                                HouseFreeCompanyName,
                                fcHouseInfo.WardIndex + 1,
                                fcHouseInfo.PlotIndex + 1
                            )
                        )
                    );
                    continue;
                }

                // 个人房屋
                case 1160:
                {
                    var personalHouseInfo = HousingManager.GetOwnedHouseId(EstateType.PersonalEstate);
                    if (personalHouseInfo.Id == INVALID_HOUSE_ID) continue;

                    var personalMarker = housingMarkers.FirstOrDefault(x => x.SubrowId == personalHouseInfo.PlotIndex);
                    if (personalMarker.RowId == 0) continue;

                    Records[VERSION_OTHER].Add
                    (
                        new AetheryteRecord
                        (
                            aetheryte.AetheryteID,
                            aetheryte.SubIndex,
                            aetheryteRow.AethernetGroup,
                            0,
                            aetheryte.TerritoryID,
                            personalMarker.Map.RowId,
                            true,
                            new(personalMarker.X, personalMarker.Y, personalMarker.Z),
                            string.Format
                            (
                                HouseEstateTemplate,
                                territoryName,
                                HousePersonalName,
                                personalHouseInfo.WardIndex + 1,
                                personalHouseInfo.PlotIndex + 1
                            )
                        )
                    );
                    break;
                }
            }

        }
    }

    #region 常量

    private static readonly string VERSION_OTHER = LuminaWrapper.GetAddonText(832);

    #endregion
}

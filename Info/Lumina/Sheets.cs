using Lumina.Excel.Sheets;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Info.Lumina;

public static class Sheets
{
    [DataShareTag]
    private const string DISPELLABLE_STATUSES_TAG = "OmenTools.Info.Game.Data.Sheets.DispellableStatuses";

    public static Dictionary<uint, Status> DispellableStatuses { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            DISPELLABLE_STATUSES_TAG,
            () => LuminaGetter.Get<Status>()
                              .Where(x => x is { CanDispel: true } && !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, s => s)
        );

    [DataShareTag]
    private const string PLAYER_ACTIONS_TAG = "OmenTools.Info.Game.Data.Sheets.PlayerActions";

    public static Dictionary<uint, Action> PlayerActions { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            PLAYER_ACTIONS_TAG,
            () => LuminaGetter.Get<Action>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .Where(x => !string.IsNullOrEmpty(x.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty))
                              .Where
                              (x => x is
                                        {
                                            IsPlayerAction: false,
                                            ClassJobLevel : > 0
                                        }
                                        or
                                        {
                                            IsPlayerAction: true
                                        }
                              )
                              .OrderBy(x => x.ClassJob.RowId)
                              .ThenBy(x => x.ClassJobLevel)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string STATUSES_TAG = "OmenTools.Info.Game.Data.Sheets.Statuses";

    public static Dictionary<uint, Status> Statuses { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            STATUSES_TAG,
            () => LuminaGetter.Get<Status>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string CONTENTS_TAG = "OmenTools.Info.Game.Data.Sheets.Contents";

    public static Dictionary<uint, ContentFinderCondition> Contents { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            CONTENTS_TAG,
            () => LuminaGetter.Get<ContentFinderCondition>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .DistinctBy(x => x.TerritoryType.RowId)
                              .OrderBy(x => x.ContentType.RowId)
                              .ThenBy(x => x.ClassJobLevelRequired)
                              .ToDictionary(x => x.TerritoryType.RowId, x => x)
        );

    [DataShareTag]
    private const string GEARS_TAG = "OmenTools.Info.Game.Data.Sheets.Gears";

    public static Dictionary<uint, Item> Gears { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            GEARS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.EquipSlotCategory.Value.RowId != 0)
                              .DistinctBy(x => x.RowId)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string WORLDS_TAG = "OmenTools.Info.Game.Data.Sheets.Worlds";

    public static Dictionary<uint, World> Worlds { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            WORLDS_TAG,
            () => LuminaGetter.Get<World>()
                              .Where
                              (x => x.DataCenter.RowId != 0                          &&
                                    x.DataCenter.RowId != 13                         && // 北美云服务器
                                    x.UserType         != 0                          &&
                                    x.Region           != 0                          &&
                                    !string.IsNullOrEmpty(x.Name.ToString())         &&
                                    !string.IsNullOrEmpty(x.InternalName.ToString()) &&
                                    !x.Name.ToString().Contains('-')                 &&
                                    !x.Name.ToString().Contains("Test")              && // 繁中测试服务器
                                    (x.Region - 1) * 1000 is var minWorldID          &&
                                    x.Region       * 1000 is var maxWorldID          &&
                                    x.RowId > minWorldID                             &&
                                    x.RowId < maxWorldID
                              )
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string CN_WORLDS_TAG = "OmenTools.Info.Game.Data.Sheets.CNWorlds";

    public static Dictionary<uint, World> CNWorlds { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            CN_WORLDS_TAG,
            () => Worlds
                  .Where
                  (x => x.Key is > 1000 and < 2000                   &&
                        x.Value.DataCenter.RowId              != 0   &&
                        x.Value.Region                        == 2   &&
                        x.Value.DataCenter.Value.Region.RowId == 5   &&
                        x.Value.UserType                      == 101 &&
                        x.Key                                 != 1200 // 排除“亚马乌罗提”
                  )
                  .ToDictionary(x => x.Key, x => x.Value)
        );

    [DataShareTag]
    private const string ZONES_TAG = "OmenTools.Info.Game.Data.Sheets.Zones";

    public static Dictionary<uint, TerritoryType> Zones { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            ZONES_TAG,
            () => LuminaGetter.Get<TerritoryType>()
                              .Where(x => x.PlaceName.RowId > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string MOUNTS_TAG = "OmenTools.Info.Game.Data.Sheets.Mounts";

    public static Dictionary<uint, Mount> Mounts { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            MOUNTS_TAG,
            () => LuminaGetter.Get<Mount>()
                              .Where(x => !string.IsNullOrEmpty(x.Singular.ToString()) && x.Icon > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string FOOD_TAG = "OmenTools.Info.Game.Data.Sheets.Food";

    public static Dictionary<uint, Item> Food { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            FOOD_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 5)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string SEEDS_TAG = "OmenTools.Info.Game.Data.Sheets.Seeds";

    public static Dictionary<uint, Item> Seeds { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            SEEDS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 20)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string SOILS_TAG = "OmenTools.Info.Game.Data.Sheets.Soils";

    public static Dictionary<uint, Item> Soils { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            SOILS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 21)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string FERTILIZERS_TAG = "OmenTools.Info.Game.Data.Sheets.Fertilizers";

    public static Dictionary<uint, Item> Fertilizers { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            FERTILIZERS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 22)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string MATERIAS_TAG = "OmenTools.Info.Game.Data.Sheets.Materias";

    public static Dictionary<uint, Item> Materias { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            MATERIAS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 13)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string SPEED_DETECTION_ZONES_TAG = "OmenTools.Info.Game.Data.Sheets.SpeedDetectionZones";

    public static Dictionary<uint, TerritoryType> SpeedDetectionZones { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            SPEED_DETECTION_ZONES_TAG,
            () =>
            {
                HashSet<uint> limitedIntendedUse = !GameState.IsGL
                                                       ? [1, 18, 31, 41, 47, 48, 52, 53, 61]
                                                       : [18, 31, 41, 48, 52, 53];
                return LuminaGetter.Get<TerritoryType>()
                                   .Where(x => limitedIntendedUse.Contains(x.TerritoryIntendedUse.RowId))
                                   .ToDictionary(x => x.RowId, x => x);
            }
        );

    [DataShareTag]
    private const string AETHERYTES_TAG = "OmenTools.Info.Game.Data.Sheets.Aetherytes";

    public static Dictionary<uint, Aetheryte> Aetherytes { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            AETHERYTES_TAG,
            () => LuminaGetter.Get<Aetheryte>()
                              .Where(x => !string.IsNullOrEmpty(x.PlaceName.ValueNullable?.Name.ExtractText()))
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string TARGET_AREA_ACTIONS_TAG = "OmenTools.Info.Game.Data.Sheets.TargetAreaActions";

    public static Dictionary<uint, Action> TargetAreaActions { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            TARGET_AREA_ACTIONS_TAG,
            () => LuminaGetter.Get<Action>()
                              .Where(x => x.TargetArea)
                              .ToDictionary(x => x.RowId, x => x)
        );
    
    [DataShareTag]
    private const string MAP_TO_FINAL_TEXTURE_MAP_TAG = "OmenTools.Info.Game.Data.Sheets.MapToFinalTextureMap";

    public static Dictionary<uint, Map> MapToFinalTextureMap { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            MAP_TO_FINAL_TEXTURE_MAP_TAG,
            () =>
            {
                var maps = LuminaGetter.Get<Map>();

                var result = new Dictionary<uint, Map>
                (
                    maps.TryGetNonEnumeratedCount(out var count) ? count : 0
                );

                var     groupRowIDs   = new List<uint>();
                string? groupTypeID   = null;
                Map     groupFinalMap = default;
                uint    previousRowID = 0;

                foreach (var map in maps)
                {
                    var mapID = map.Id.ToString();

                    var sameGroup =
                        groupRowIDs.Count > 0                  &&
                        map.RowId         == previousRowID + 1 &&
                        TypePart(mapID).Equals(TypePart(groupTypeID), StringComparison.Ordinal);

                    if (!sameGroup)
                    {
                        FlushGroup();

                        groupTypeID = mapID;
                        groupRowIDs.Clear();
                    }

                    groupRowIDs.Add(map.RowId);
                    groupFinalMap = map;
                    previousRowID = map.RowId;
                }

                FlushGroup();

                return result;

                void FlushGroup()
                {
                    if (groupRowIDs.Count == 0)
                        return;

                    foreach (var rowID in groupRowIDs)
                        result[rowID] = groupFinalMap;
                }

                static ReadOnlySpan<char> TypePart(string? id)
                {
                    var span  = id.AsSpan();
                    var slash = span.IndexOf('/');

                    return slash >= 0
                               ? span[..slash]
                               : span;
                }
            }
        );
    
    [DataShareTag]
    private const string PLAYER_SEARCH_PLACE_NAME_TO_ZONES_TAG = "OmenTools.Info.Game.Data.Sheets.PlayerSearchPlaceNameToZones";
    
    public static Dictionary<uint, HashSet<uint>> PlayerSearchPlaceNameToZones { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            PLAYER_SEARCH_PLACE_NAME_TO_ZONES_TAG,
            () => LuminaGetter.Get<PlayerSearchSubLocation>()
                              .Where(x => x.PlaceName.RowId is > 0 and not 519)
                              .Select(x => x.PlaceName.RowId)
                              .Distinct()
                              .ToDictionary
                              (
                                  x => x,
                                  x =>
                                      LuminaGetter.Get<TerritoryType>()
                                                  .Where
                                                  (d => d.PlaceNameZone.RowId == x &&
                                                        d.TerritoryIntendedUse.RowId is 0 or 1 or 23
                                                  ) // 野外、主城和金碟
                                                  .Select(d => d.RowId)
                                                  .ToHashSet()
                              )
        );
    
    [DataShareTag]
    private const string PLAYER_SEARCH_PLACE_NAMES_TAG = "OmenTools.Info.Game.Data.Sheets.PlayerSearchPlaceNames";

    public static Dictionary<uint, PlayerSearchSubLocation> PlayerSearchPlaceNames { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            PLAYER_SEARCH_PLACE_NAMES_TAG,
            () => LuminaGetter.Get<PlayerSearchSubLocation>()
                              .Where(x => x.PlaceName.RowId is > 0 and not 519)
                              .DistinctBy(x => x.PlaceName.RowId)
                              .ToDictionary
                              (
                                  x => x.PlaceName.RowId,
                                  x => x
                              )
        );
}

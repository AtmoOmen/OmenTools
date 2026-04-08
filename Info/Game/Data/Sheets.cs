using Lumina.Excel.Sheets;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.Info.Models;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Info.Game.Data;

public static class Sheets
{
    [DataShareTag]
    private const string DISPELLABLE_STATUSES_TAG = "OmenTools.Info.Game.Data.Sheets.DispellableStatuses";
    
    public static Dictionary<uint, Status> DispellableStatuses { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            DISPELLABLE_STATUSES_TAG,
            () => LuminaGetter.Get<Status>()
                              .Where(x => x is { CanDispel: true } && !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, s => s)
        );

    [DataShareTag]
    private const string PLAYER_ACTIONS_TAG = "OmenTools.Info.Game.Data.Sheets.PlayerActions";
    
    public static Dictionary<uint, Action> PlayerActions { get; } =
        DService.Instance().PI.GetOrCreateData
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
        DService.Instance().PI.GetOrCreateData
        (
            STATUSES_TAG,
            () => LuminaGetter.Get<Status>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, x => x)
        );
    
    [DataShareTag]
    private const string CONTENTS_TAG = "OmenTools.Info.Game.Data.Sheets.Contents";

    public static Dictionary<uint, ContentFinderCondition> Contents { get; } =
        DService.Instance().PI.GetOrCreateData
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
        DService.Instance().PI.GetOrCreateData
        (
            GEARS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.EquipSlotCategory.Value.RowId != 0)
                              .DistinctBy(x => x.RowId)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string DYES_TAG = "OmenTools.Info.Game.Data.Sheets.Dyes";
    
    public static Dictionary<uint, Item> Dyes { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            DYES_TAG,
            () => LuminaGetter.Get<StainTransient>()
                              .Where(x => x.Item1.ValueNullable != null)
                              .ToDictionary(x => x.RowId, x => x.Item1.Value)
        );

    [DataShareTag]
    private const string WORLDS_TAG = "OmenTools.Info.Game.Data.Sheets.Worlds";
    
    public static Dictionary<uint, World> Worlds { get; } =
        DService.Instance().PI.GetOrCreateData
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
                                    (x.Region - 1) * 1000 is var minWorldID       &&
                                    x.Region       * 1000 is var maxWorldID       &&
                                    x.RowId > minWorldID                             &&
                                    x.RowId < maxWorldID
                              )
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string CN_WORLDS_TAG = "OmenTools.Info.Game.Data.Sheets.CNWorlds";
    
    public static Dictionary<uint, World> CNWorlds { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            CN_WORLDS_TAG,
            () => Worlds
                  .Where
                  (x => x.Key is > 1000 and < 2000           &&
                        x.Value.DataCenter.RowId        != 0 &&
                        x.Value.Region                  == 2 &&
                        x.Value.DataCenter.Value.Region == 5 &&
                        x.Value.UserType                == 101
                  )
                  .ToDictionary(x => x.Key, x => x.Value)
        );

    [DataShareTag]
    private const string ZONES_TAG = "OmenTools.Info.Game.Data.Sheets.Zones";
    
    public static Dictionary<uint, TerritoryType> Zones { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            ZONES_TAG,
            () => LuminaGetter.Get<TerritoryType>()
                              .Where(x => x.PlaceName.RowId > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );
    
    [DataShareTag]
    private const string MOUNTS_TAG = "OmenTools.Info.Game.Data.Sheets.Mounts";

    public static Dictionary<uint, Mount> Mounts { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            MOUNTS_TAG,
            () => LuminaGetter.Get<Mount>()
                              .Where(x => !string.IsNullOrEmpty(x.Singular.ToString()) && x.Icon > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string FOOD_TAG = "OmenTools.Info.Game.Data.Sheets.Food";
    
    public static Dictionary<uint, Item> Food { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            FOOD_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 5)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string SEEDS_TAG = "OmenTools.Info.Game.Data.Sheets.Seeds";
    
    public static Dictionary<uint, Item> Seeds { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            SEEDS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 20)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string SOILS_TAG = "OmenTools.Info.Game.Data.Sheets.Soils";
    
    public static Dictionary<uint, Item> Soils { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            SOILS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 21)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string FERTILIZERS_TAG = "OmenTools.Info.Game.Data.Sheets.Fertilizers";
    
    public static Dictionary<uint, Item> Fertilizers { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            FERTILIZERS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 22)
                              .ToDictionary(x => x.RowId, x => x)
        );

    [DataShareTag]
    private const string MATERIAS_TAG = "OmenTools.Info.Game.Data.Sheets.Materias";
    
    public static Dictionary<uint, Item> Materias { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            MATERIAS_TAG,
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 13)
                              .ToDictionary(x => x.RowId, x => x)
        );
    
    [DataShareTag]
    private const string SPEED_DETECTION_ZONES_TAG = "OmenTools.Info.Game.Data.Sheets.SpeedDetectionZones";

    public static Dictionary<uint, TerritoryType> SpeedDetectionZones { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            SPEED_DETECTION_ZONES_TAG,
            () =>
            {
                HashSet<uint> limitedIntendedUse = GameState.IsCN || GameState.IsTC
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
        DService.Instance().PI.GetOrCreateData
        (
            AETHERYTES_TAG,
            () => LuminaGetter.Get<Aetheryte>()
                              .Where(x => !string.IsNullOrEmpty(x.PlaceName.ValueNullable?.Name.ExtractText()))
                              .ToDictionary(x => x.RowId, x => x)
        );
    
    [DataShareTag]
    private const string TARGET_AREA_ACTIONS_TAG = "OmenTools.Info.Game.Data.Sheets.TargetAreaActions";
    
    public static Dictionary<uint, Action> TargetAreaActions { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            TARGET_AREA_ACTIONS_TAG,
            () => LuminaGetter.Get<Action>()
                              .Where(x => x.TargetArea)
                              .ToDictionary(x => x.RowId, x => x)
        );
}

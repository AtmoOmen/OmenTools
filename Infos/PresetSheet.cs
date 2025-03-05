using Dalamud.Interface.Textures;
using Lumina.Excel.Sheets;
using OmenTools.Helpers;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Infos;

public class PresetSheet
{
    public static Dictionary<uint, Action>                 PlayerActions   => playerActions.Value;
    public static Dictionary<uint, Status>                 Statuses        => statuses.Value;
    public static Dictionary<uint, ContentFinderCondition> Contents        => contents.Value;
    public static Dictionary<uint, Item>                   Gears           => gears.Value;
    public static Dictionary<uint, Item>                   Dyes            => dyes.Value;
    public static Dictionary<uint, World>                  Worlds          => worlds.Value;
    public static Dictionary<uint, World>                  CNWorlds        => cnWorlds.Value;
    public static Dictionary<uint, TerritoryType>          Zones           => zones.Value;
    public static Dictionary<uint, Mount>                  Mounts          => mounts.Value;
    public static Dictionary<uint, Item>                   Food            => food.Value;
    public static Dictionary<uint, Item>                   Seeds           => seeds.Value;
    public static Dictionary<uint, Item>                   Soils           => soils.Value;
    public static Dictionary<uint, Item>                   Fertilizers     => fertilizers.Value;
    public static Dictionary<uint, Item>                   Materias        => materias.Value;

    public static bool TryGetContent(uint rowID, out ContentFinderCondition content)
        => Contents.TryGetValue(rowID, out content);

    public static bool TryGetGear(uint rowID, out Item item)
        => Gears.TryGetValue(rowID, out item);

    public static bool TryGetCNWorld(uint rowID, out World world)
        => CNWorlds.TryGetValue(rowID, out world);

    #region Lazy

    private static readonly Lazy<Dictionary<uint, World>> worlds =
        new(() => LuminaCache.Get<World>()
                             .Where(x => x.DataCenter.ValueNullable != null &&
                                         (x.DataCenter.ValueNullable?.Region ?? 0) != 0 &&
                                         !string.IsNullOrWhiteSpace(x.DataCenter.ValueNullable?.Name.ExtractText()) &&
                                         !string.IsNullOrWhiteSpace(x.Name.ExtractText()) &&
                                         !string.IsNullOrWhiteSpace(x.InternalName.ExtractText()) &&
                                         !x.Name.ExtractText().Contains('-') &&
                                         !x.Name.ExtractText().Contains('_'))
                             .Where(x => x.DataCenter.Value.Region != 5 || 
                                         (x.RowId > 1000 && x.RowId != 1200 && 
                                          IsChineseString(x.Name.ExtractText())))
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> seeds =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => x.FilterGroup == 20)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> soils =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => x.FilterGroup == 21)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> fertilizers =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => x.FilterGroup == 22)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Action>> playerActions =
        new(() => LuminaCache.Get<Action>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                             .Where(x => x is
                             {
                                 IsPlayerAction: false,
                                 ClassJobLevel: > 0,
                             } or { IsPlayerAction: true })
                             .OrderBy(x => x.ClassJob.RowId)
                             .ThenBy(x => x.ClassJobLevel)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Status>> statuses =
        new(() => LuminaCache.Get<Status>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, ContentFinderCondition>> contents =
        new(() => LuminaCache.Get<ContentFinderCondition>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                             .DistinctBy(x => x.TerritoryType.RowId)
                             .OrderBy(x => x.ContentType.RowId)
                             .ThenBy(x => x.ClassJobLevelRequired)
                             .ToDictionary(x => x.TerritoryType.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> gears =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => x.EquipSlotCategory.Value.RowId != 0)
                             .DistinctBy(x => x.RowId)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> dyes =
        new(() => LuminaCache.Get<StainTransient>()
                             .Where(x => x.Item1.ValueNullable != null)
                             .ToDictionary(x => x.RowId, x => x.Item1.Value));

    private static readonly Lazy<Dictionary<uint, World>> cnWorlds =
        new(() => LuminaCache.Get<World>()
                             .Where(x => x.DataCenter.Value.Region == 5 &&
                                         x.RowId > 1000 && x.RowId != 1200 &&
                                         !string.IsNullOrWhiteSpace(x.Name.ExtractText()) &&
                                         !string.IsNullOrWhiteSpace(x.InternalName.ExtractText()) &&
                                         IsChineseString(x.Name.ExtractText()))
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, TerritoryType>> zones =
        new(() => LuminaCache.Get<TerritoryType>()
                             .Where(x => x.PlaceName.RowId > 0)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Mount>> mounts =
        new(() => LuminaCache.Get<Mount>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Singular.ExtractText()) && x.Icon > 0)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> food =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()) && x.FilterGroup == 5)
                             .ToDictionary(x => x.RowId, x => x));

    private static readonly Lazy<Dictionary<uint, Item>> materias =
        new(() => LuminaCache.Get<Item>()
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()) && x.FilterGroup == 13)
                             .ToDictionary(x => x.RowId, x => x));

    #endregion
}

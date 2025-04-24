using Lumina.Excel.Sheets;
using OmenTools.Helpers;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Infos;

public static class PresetSheet
{
    /// <summary>
    /// 可驱散的状态效果
    /// </summary>
    public static Dictionary<uint, Status> DispellableStatuses { get; } = 
        LuminaGetter.Get<Status>()
                    .Where(x => x is { CanDispel: true } && !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                    .ToDictionary(x => x.RowId, s => s);
    
    public static Dictionary<uint, Action> PlayerActions { get; } =
        LuminaGetter.Get<Action>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                    .Where(x => x is
                    {
                        IsPlayerAction: false,
                        ClassJobLevel : > 0
                    } 
                    or
                    {
                        IsPlayerAction: true
                    })
                    .OrderBy(x => x.ClassJob.RowId)
                    .ThenBy(x => x.ClassJobLevel)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Status> Statuses { get; } =
        LuminaGetter.Get<Status>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, ContentFinderCondition> Contents { get; } =
        LuminaGetter.Get<ContentFinderCondition>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()))
                    .DistinctBy(x => x.TerritoryType.RowId)
                    .OrderBy(x => x.ContentType.RowId)
                    .ThenBy(x => x.ClassJobLevelRequired)
                    .ToDictionary(x => x.TerritoryType.RowId, x => x);

    public static Dictionary<uint, Item> Gears { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => x.EquipSlotCategory.Value.RowId != 0)
                    .DistinctBy(x => x.RowId)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Dyes { get; } =
        LuminaGetter.Get<StainTransient>()
                    .Where(x => x.Item1.ValueNullable != null)
                    .ToDictionary(x => x.RowId, x => x.Item1.Value);

    public static Dictionary<uint, World> Worlds { get; } =
        LuminaGetter.Get<World>()
                    .Where(x => x.DataCenter.ValueNullable                != null                          &&
                                (x.DataCenter.ValueNullable?.Region ?? 0) != 0                             &&
                                !string.IsNullOrWhiteSpace(x.DataCenter.ValueNullable?.Name.ExtractText()) &&
                                !string.IsNullOrWhiteSpace(x.Name.ExtractText())                           &&
                                !string.IsNullOrWhiteSpace(x.InternalName.ExtractText())                   &&
                                !x.Name.ExtractText().Contains('-')                                        &&
                                !x.Name.ExtractText().Contains('_'))
                    .Where(x => x.DataCenter.Value.Region != 5 ||
                                (x.RowId > 1000 && x.RowId != 1200 &&
                                 IsChineseString(x.Name.ExtractText())))
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, World> CNWorlds { get; } =
        LuminaGetter.Get<World>()
                    .Where(x => x.DataCenter.Value.Region == 5                           &&
                                x.RowId                   > 1000                         && x.RowId != 1200 &&
                                !string.IsNullOrWhiteSpace(x.Name.ExtractText())         &&
                                !string.IsNullOrWhiteSpace(x.InternalName.ExtractText()) &&
                                IsChineseString(x.Name.ExtractText()))
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, TerritoryType> Zones { get; } =
        LuminaGetter.Get<TerritoryType>()
                    .Where(x => x.PlaceName.RowId > 0)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Mount> Mounts { get; } =
        LuminaGetter.Get<Mount>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Singular.ExtractText()) && x.Icon > 0)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Food { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()) && x.FilterGroup == 5)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Seeds { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => x.FilterGroup == 20)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Soils { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => x.FilterGroup == 21)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Fertilizers { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => x.FilterGroup == 22)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Materias { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name.ExtractText()) && x.FilterGroup == 13)
                    .ToDictionary(x => x.RowId, x => x);
}

using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Infos;

public static class PresetSheet
{
    public static Dictionary<uint, Status> DispellableStatuses { get; } = 
        LuminaGetter.Get<Status>()
                    .Where(x => x is { CanDispel: true } && !string.IsNullOrEmpty(x.Name.ToString()))
                    .ToDictionary(x => x.RowId, s => s);
    
    public static Dictionary<uint, Action> PlayerActions { get; } =
        LuminaGetter.Get<Action>()
                    .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                    .Where(x => !string.IsNullOrEmpty(x.ClassJobCategory.ValueNullable?.Name.ToString() ?? string.Empty))
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
                    .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, ContentFinderCondition> Contents { get; } =
        LuminaGetter.Get<ContentFinderCondition>()
                    .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
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
                    .Where(x => x.DataCenter.RowId != 0                          &&
                                x.DataCenter.RowId != 13                         && // 北美云服务器
                                x.UserType         != 0                          &&
                                x.Region           != 0                          &&
                                !string.IsNullOrEmpty(x.Name.ToString())         &&
                                !string.IsNullOrEmpty(x.InternalName.ToString()) &&
                                !x.Name.ToString().Contains("-"))
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, World> CNWorlds { get; } =
        Worlds
            .Where(x => x.Key is > 1000 and < 2000           &&
                        x.Value.DataCenter.RowId        != 0 &&
                        x.Value.UserType                == 2 &&
                        x.Value.DataCenter.Value.Region == 5 &&
                        x.Value.Region                  == 101)
            .ToDictionary(x => x.Key, x => x.Value);

    public static Dictionary<uint, TerritoryType> Zones { get; } =
        LuminaGetter.Get<TerritoryType>()
                    .Where(x => x.PlaceName.RowId > 0)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Mount> Mounts { get; } =
        LuminaGetter.Get<Mount>()
                    .Where(x => !string.IsNullOrEmpty(x.Singular.ToString()) && x.Icon > 0)
                    .ToDictionary(x => x.RowId, x => x);

    public static Dictionary<uint, Item> Food { get; } =
        LuminaGetter.Get<Item>()
                    .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 5)
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
                    .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 13)
                    .ToDictionary(x => x.RowId, x => x);
}

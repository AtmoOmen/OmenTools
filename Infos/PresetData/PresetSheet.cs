using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace OmenTools.Infos;

public static class PresetSheet
{
    public static Dictionary<uint, Status> DispellableStatuses { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.DispellableStatuses",
            () => LuminaGetter.Get<Status>()
                              .Where(x => x is { CanDispel: true } && !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, s => s)
        );

    public static Dictionary<uint, Action> PlayerActions { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.PlayerActions",
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

    public static Dictionary<uint, Status> Statuses { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Statuses",
            () => LuminaGetter.Get<Status>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, ContentFinderCondition> Contents { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Contents",
            () => LuminaGetter.Get<ContentFinderCondition>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()))
                              .DistinctBy(x => x.TerritoryType.RowId)
                              .OrderBy(x => x.ContentType.RowId)
                              .ThenBy(x => x.ClassJobLevelRequired)
                              .ToDictionary(x => x.TerritoryType.RowId, x => x)
        );

    public static Dictionary<uint, Item> Gears { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Gears",
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.EquipSlotCategory.Value.RowId != 0)
                              .DistinctBy(x => x.RowId)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Dyes { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Dyes",
            () => LuminaGetter.Get<StainTransient>()
                              .Where(x => x.Item1.ValueNullable != null)
                              .ToDictionary(x => x.RowId, x => x.Item1.Value)
        );

    public static Dictionary<uint, World> Worlds { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Worlds",
            () => LuminaGetter.Get<World>()
                              .Where
                              (x => x.DataCenter.RowId != 0                          &&
                                    x.DataCenter.RowId != 13                         && // 北美云服务器
                                    x.UserType         != 0                          &&
                                    x.Region           != 0                          &&
                                    !string.IsNullOrEmpty(x.Name.ToString())         &&
                                    !string.IsNullOrEmpty(x.InternalName.ToString()) &&
                                    !x.Name.ToString().Contains('-')                 &&
                                    (x.Region - 1) * 1000 is var minWorldID          &&
                                    x.Region       * 1000 is var maxWorldID          &&
                                    x.RowId > minWorldID                             &&
                                    x.RowId < maxWorldID
                              )
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, World> CNWorlds { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.CNWorlds",
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

    public static Dictionary<uint, TerritoryType> Zones { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Zones",
            () => LuminaGetter.Get<TerritoryType>()
                              .Where(x => x.PlaceName.RowId > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Mount> Mounts { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Mounts",
            () => LuminaGetter.Get<Mount>()
                              .Where(x => !string.IsNullOrEmpty(x.Singular.ToString()) && x.Icon > 0)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Food { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Food",
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 5)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Seeds { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Seeds",
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 20)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Soils { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Soils",
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 21)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Fertilizers { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Fertilizers",
            () => LuminaGetter.Get<Item>()
                              .Where(x => x.FilterGroup == 22)
                              .ToDictionary(x => x.RowId, x => x)
        );

    public static Dictionary<uint, Item> Materias { get; } =
        DService.Instance().PI.GetOrCreateData
        (
            "OmenTools.Infos.PresetSheet.Materias",
            () => LuminaGetter.Get<Item>()
                              .Where(x => !string.IsNullOrEmpty(x.Name.ToString()) && x.FilterGroup == 13)
                              .ToDictionary(x => x.RowId, x => x)
        );
}

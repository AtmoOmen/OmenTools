using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.Info.Lumina.Enums;
using OmenTools.Info.Lumina.LGB;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Lumina;

public static class LGBAssets
{
    [DataShareTag]
    private const string ZONE_LINE_EXIT_RANGES_TAG = "OmenTools.Info.Game.Data.LGBAssets.ZoneLineExitRanges";

    public static Dictionary<uint, List<ZoneLineExitRange>> ZoneLineExitRanges { get; } =
        IDalamudPluginInterface.Instance().GetOrCreateData
        (
            ZONE_LINE_EXIT_RANGES_TAG,
            () =>
            {
                var dict = new Dictionary<uint, List<ZoneLineExitRange>>();

                foreach (var zone in LuminaGetter.Get<TerritoryType>())
                {
                    if (zone.GetLGB(LGBFileType.PlanMap)?.GetInstanceObjects(LayerEntryType.ExitRange) is not { Count: > 0 } exitRanges)
                        continue;

                    var list = new List<ZoneLineExitRange>();

                    foreach (var er in exitRanges)
                    {
                        var exitRange = (LayerCommon.ExitRangeInstanceObject)er.Object;
                        if (exitRange.ReturnInstanceId == 0) continue;

                        list.Add(new(zone.RowId, exitRange.TerritoryType, er.GetPosition()));
                    }

                    dict[zone.RowId] = list;
                }

                return dict.ToDictionary();
            }
        );
}

using System.Collections.Frozen;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.Info.Game.AetheryteRecord.Data;

public static partial class AetheryteRecords
{
    public static FrozenSet<byte> AethernetGroups { get; } =
        LuminaGetter.Get<Aetheryte>()
                    .Select(x => x.AethernetGroup)
                    .ToFrozenSet();
}

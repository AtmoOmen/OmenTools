using System.Collections.Frozen;

namespace OmenTools.Info.Game.Data.Icons;

public static class FateIcons
{
    public static FrozenSet<uint> All { get; } = [60458, 60501, 60502, 60503, 60504, 60505, 60506, 60507, 60508];

    public static bool IsFate(uint iconID) =>
        All.Contains(iconID);
}

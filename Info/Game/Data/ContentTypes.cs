using System.Collections.Frozen;

namespace OmenTools.Info.Game.Data;

public static class ContentTypes
{
    public static FrozenSet<uint> NotPVE { get; } = [6, 12, 16, 17, 18, 19, 20, 25, 31, 32, 34, 35, 36];
}

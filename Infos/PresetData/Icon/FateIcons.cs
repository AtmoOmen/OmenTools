namespace OmenTools.Infos;

public static class FateIcons
{
    public static readonly HashSet<uint> All = [60458, 60501, 60502, 60503, 60504, 60505, 60506, 60507, 60508];

    public static bool IsFate(uint iconID) =>
        All.Contains(iconID);
}

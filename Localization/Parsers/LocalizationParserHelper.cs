using System.Collections.Frozen;

namespace OmenTools.Localization.Parsers;

internal static class LocalizationParserHelper
{
    public static FrozenDictionary<string, string> EmptyResource { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal).ToFrozenDictionary(StringComparer.Ordinal);

    public static FrozenDictionary<string, string> ToFrozenResource(this Dictionary<string, string> dict) =>
        dict.Count == 0 ? EmptyResource : dict.ToFrozenDictionary(StringComparer.Ordinal);
}

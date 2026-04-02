using System.Collections.Frozen;
using System.Text;
using Newtonsoft.Json;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class JsonDictionaryLocalizationParser : ILocalizationParser
{
    private static readonly FrozenDictionary<string, string> EmptyResource =
        new Dictionary<string, string>(StringComparer.Ordinal).ToFrozenDictionary(StringComparer.Ordinal);

    public FrozenDictionary<string, string> Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader     = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader);

        var serializer = JsonSerializer.CreateDefault();
        var dict       = serializer.Deserialize<Dictionary<string, string>>(jsonReader) ?? [];

        return dict.Count == 0
                   ? EmptyResource
                   : dict.ToFrozenDictionary(StringComparer.Ordinal);
    }
}

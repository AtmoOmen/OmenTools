using System.Collections.Frozen;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class StructuredJsonLocalizationParser
(
    string keyDelimiter = "."
) : ILocalizationParser
{
    private readonly string keyDelimiter = keyDelimiter;

    public FrozenDictionary<string, string> Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader     = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader);

        var                        root   = JToken.ReadFrom(jsonReader);
        Dictionary<string, string> result = new(StringComparer.Ordinal);

        if (root is JObject obj)
            VisitObject(obj, null, result);

        return result.ToFrozenResource();
    }

    private void VisitObject(JObject obj, string? prefix, Dictionary<string, string> result)
    {
        foreach (var property in obj.Properties())
        {
            var currentKey = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}{keyDelimiter}{property.Name}";

            switch (property.Value)
            {
                case JObject childObject when TryGetStringValue(childObject, out var value):
                    result[currentKey] = value;
                    break;
                case JObject childObject:
                    VisitObject(childObject, currentKey, result);
                    break;
                case JValue { Type: JTokenType.String } stringValue:
                    result[currentKey] = stringValue.Value<string>() ?? string.Empty;
                    break;
            }
        }
    }

    private static bool TryGetStringValue(JObject obj, out string value)
    {
        value = string.Empty;

        if (obj.TryGetValue("string", out var token) && token.Type == JTokenType.String)
        {
            value = token.Value<string>() ?? string.Empty;
            return true;
        }

        return false;
    }
}

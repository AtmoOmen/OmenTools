using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class KeyValueJsonLocalizationParser
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
            VisitToken(obj, null, result);

        return result.ToFrozenResource();
    }

    private void VisitToken(JToken token, string? prefix, Dictionary<string, string> result)
    {
        switch (token)
        {
            case JObject obj:
                foreach (var property in obj.Properties())
                {
                    var currentKey = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}{keyDelimiter}{property.Name}";
                    VisitToken(property.Value, currentKey, result);
                }

                break;
            case JArray array:
                for (var index = 0; index < array.Count; index++)
                    VisitToken(array[index], $"{prefix}[{index}]", result);

                break;
            case JValue value when prefix != null:
                result[prefix] = ConvertToString(value);
                break;
        }
    }

    private static string ConvertToString(JValue value) =>
        value.Type switch
        {
            JTokenType.Null    => string.Empty,
            JTokenType.String  => value.Value<string>() ?? string.Empty,
            JTokenType.Boolean => value.Value<bool>() ? "true" : "false",
            _                  => Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty
        };
}
